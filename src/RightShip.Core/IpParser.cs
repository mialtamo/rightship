using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace RightShip.Core;

public static class IpParser
{
    public static ParsedAddresses Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var valid = new List<string>();
        var errors = new List<ValidationIssue>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var rows = 0;
        var duplicates = 0;
        var lines = text.Replace("\r", "").Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            // Plain text / CSV first column. Blank lines and # comments are ignored.
            var line = lines[i].Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;
            foreach (var field in line.Split(','))
            {
                var entry = field.Trim().Trim('"');
                if (entry.Length == 0) continue;
                rows++;
                if (!TryCanonicalize(entry, out var canonical))
                {
                    errors.Add(new ValidationIssue(i + 1, entry, "Invalid IP literal or CIDR prefix"));
                    continue;
                }
                if (!seen.Add(canonical)) { duplicates++; continue; }
                valid.Add(canonical);
            }
        }
        return new ParsedAddresses(rows, valid, errors, duplicates);
    }

    public static bool TryCanonicalize(string value, out string canonical)
    {
        canonical = "";
        var pieces = value.Split('/');
        if (pieces.Length is < 1 or > 2 || !IPAddress.TryParse(pieces[0], out var address)) return false;
        // Do not allow IPv4-integer/shortened notation or scoped IPv6 addresses.
        if (address.ScopeId != 0) return false;
        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            if (pieces[0].Split('.').Length != 4 || pieces[0].Split('.').Any(x => x.Length == 0 || !x.All(char.IsAsciiDigit) || (x.Length > 1 && x[0] == '0'))) return false;
        }
        var max = address.AddressFamily == AddressFamily.InterNetwork ? 32 : 128;
        if (pieces.Length == 1) { canonical = address.ToString().ToLowerInvariant(); return true; }
        if (!int.TryParse(pieces[1], out var prefix) || prefix < 0 || prefix > max) return false;
        var bytes = address.GetAddressBytes();
        for (var i = 0; i < bytes.Length; i++)
        {
            var remaining = prefix - i * 8;
            var mask = remaining >= 8 ? 255 : remaining <= 0 ? 0 : 255 << (8 - remaining) & 255;
            bytes[i] = (byte)(bytes[i] & mask);
        }
        canonical = new IPAddress(bytes).ToString().ToLowerInvariant() + "/" + prefix;
        return true;
    }
}
