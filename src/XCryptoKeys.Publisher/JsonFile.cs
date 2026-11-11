using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace XCryptoKeys.Publisher;

/// <summary>
/// Lecture et écriture des fichiers JSON du dépôt en gardant leur format :
/// même rendu que Python json.dumps(indent=2, ensure_ascii=False), même fin de ligne (LF ou CRLF),
/// même retour à la ligne final. Le diff git ne montre donc que les champs modifiés.
/// </summary>
public static class JsonFile
{
    public sealed record Format(string NewLine, bool FinalNewLine);

    public static (JsonNode Node, Format Format) Read(string path)
    {
        string text = File.ReadAllText(path, Encoding.UTF8);
        string nl = text.Contains("\r\n") ? "\r\n" : "\n";
        bool finalNl = text.EndsWith('\n');
        var node = JsonNode.Parse(text) ?? throw new InvalidDataException($"JSON vide : {path}");
        return (node, new Format(nl, finalNl));
    }

    public static void Write(string path, JsonNode node, Format format)
    {
        string text = Serialize(node, format.NewLine);
        if (format.FinalNewLine) text += format.NewLine;
        string tmp = path + ".tmp";
        File.WriteAllText(tmp, text, new UTF8Encoding(false));
        File.Move(tmp, path, overwrite: true);
    }

    public static string Serialize(JsonNode? node, string newLine = "\n")
    {
        var sb = new StringBuilder();
        WriteNode(sb, node, 0, newLine);
        return sb.ToString();
    }

    private static void WriteNode(StringBuilder sb, JsonNode? node, int depth, string nl)
    {
        switch (node)
        {
            case null:
                sb.Append("null");
                break;
            case JsonObject obj:
                if (obj.Count == 0) { sb.Append("{}"); break; }
                sb.Append('{');
                bool firstProp = true;
                foreach (var (key, value) in obj)
                {
                    if (!firstProp) sb.Append(',');
                    firstProp = false;
                    sb.Append(nl).Append(' ', (depth + 1) * 2);
                    WriteString(sb, key);
                    sb.Append(": ");
                    WriteNode(sb, value, depth + 1, nl);
                }
                sb.Append(nl).Append(' ', depth * 2).Append('}');
                break;
            case JsonArray arr:
                if (arr.Count == 0) { sb.Append("[]"); break; }
                sb.Append('[');
                for (int i = 0; i < arr.Count; i++)
                {
                    if (i > 0) sb.Append(',');
                    sb.Append(nl).Append(' ', (depth + 1) * 2);
                    WriteNode(sb, arr[i], depth + 1, nl);
                }
                sb.Append(nl).Append(' ', depth * 2).Append(']');
                break;
            case JsonValue val:
                var el = val.GetValueKind();
                if (el == JsonValueKind.String) WriteString(sb, val.GetValue<string>());
                else sb.Append(val.ToJsonString());
                break;
        }
    }

    private static void WriteString(StringBuilder sb, string s)
    {
        sb.Append('"');
        foreach (char c in s)
        {
            switch (c)
            {
                case '"': sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                case '\b': sb.Append("\\b"); break;
                case '\f': sb.Append("\\f"); break;
                default:
                    if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                    else sb.Append(c);
                    break;
            }
        }
        sb.Append('"');
    }
}
