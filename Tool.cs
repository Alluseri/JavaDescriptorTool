using System.Text;
using System.Text.RegularExpressions;

namespace DescriptorTool;

public static class Tool {
    // I know this code is straight up horrible, I'm not looking for enterprise level coding in a tool as simple as this one and I doubt anyone other than me will see it anyway
    // TODO: Option to simplify fully qualified names
    public static T[] Apply<X, T>(this IList<X> Array, Func<X, int, T> Act) {
        T[] S = new T[Array.Count];
        for (int i = 0; i < Array.Count; i++) {
            S[i] = Act.Invoke(Array[i], i);
        }
        return S;
    }

    public static string DescribeArray(this string Target) {
        StringBuilder sb = new();
        foreach (char c in Target.ToCharArray()) {
            if (c == '[')
                sb.Append('[');
        }
        if (Target.Contains("..."))
            sb.Append('[');
        return sb.ToString();
    }

    public static void Main(string[] Args) {
        Console.WriteLine("JavaDescriptorTool by Alluseri, running version 1.0");
        if (Args.Length > 1) {
            string Full = string.Concat(Args.Skip(1));
            switch (Args[0]) {
                case "-p":
                case "--parse":
                Console.WriteLine("Parsing " + Full + "...");
                Console.WriteLine(Parse(Full));
                return;
                case "-d":
                case "--describe":
                Console.WriteLine("Describing " + Full + "...");
                Console.WriteLine(Describe(Full));
                return;
                default:
                Console.WriteLine("Unknown mode. -p/--parse for parsing descriptors, -d/--describe for generating them.");
                return;
            }
        }
        Console.WriteLine("Please choose the mode you want to run DescriptorTool in for this session");
        Console.WriteLine("1 - Describe/Generate (Default)");
        Console.WriteLine("2 - Parse");
        if (Console.ReadLine()!.Contains('2')) { // Don't ask why.
            Console.WriteLine("Warning: Please keep in mind that you must only use fully qualified class names!");
            Console.WriteLine("e.g. Replace String with java/lang/String");
            Console.WriteLine("Please enter the descriptor you want to parse:");
            Console.WriteLine(Parse(Console.ReadLine()!));
        } else {
            Console.WriteLine("Please enter the full method signature:");
            Console.WriteLine(Describe(Console.ReadLine()!));
        }
    }

    private static string DescribeType(string Type, string ArrayCompound, MatchCollection? Generics) {
        switch (Type) {
            case "byte":
            return ArrayCompound + "B";
            case "char":
            return ArrayCompound + "C";
            case "double":
            return ArrayCompound + "D";
            case "float":
            return ArrayCompound + "F";
            case "int":
            return ArrayCompound + "I";
            case "long":
            return ArrayCompound + "J";
            case "short":
            return ArrayCompound + "S";
            case "boolean":
            return ArrayCompound + "Z";
            case "void":
            return ArrayCompound + "V";
            default:
            if (Generics == null)
                return ArrayCompound + "L" + Type + ";";
            else {
                Match? a = Generics.FirstOrDefault((m) => m!.Groups[1].Value.Trim().Equals(Type), null);
                return a != null ? ArrayCompound + "L" + (a.Groups[2].Length == 0 ? "java/lang/Object" : a.Groups[2].Value.Trim()) + ";" : ArrayCompound + "L" + Type + ";";
            }
        }
    }
    public static string Describe(string Pattern) {
        // TODO: Simple -> Qualified converter
        MatchCollection? Generics = null;
        Match _ = new Regex(@"<(.+?)> ").Match(Pattern);
        if (_.Success) {
            Generics = new Regex(@"(\S+?)(?:\s+extends\s+(.+?)(?:<.+?>)*){0,1}(?:,|$)").Matches(_.Groups[1].Value);
            Pattern = Pattern[_.Length..];
        }
        MatchCollection ms = new Regex(@"(.+?)((?:\[\s*\]\s*|\s*\.\.\.\s*)*)\s+[^[]+?(?:[ ,()]+)").Matches(Pattern);
        StringBuilder sb = new("(");
        if (ms.Count > 1)
            for (int i = 1; i < ms.Count; i++) {
                sb.Append(DescribeType(ms[i].Groups[1].Value.Trim(), ms[i].Groups[2].Value.DescribeArray(), Generics));
            }
        sb.Append(')');
        sb.Append(DescribeType(ms[0].Groups[1].Value.Trim(), ms[0].Groups[2].Value.DescribeArray(), Generics));
        return sb.ToString();
    }
    public static string Parse(string Pattern) {
        List<string> funcArgs = new();
        string retType = "catgirls_are_cute";
        bool outside = false;
        bool extending = false;
        StringBuilder extend = new();
        string pusher = "";
        foreach (char c in Pattern.ToCharArray()) {
            if (extending) {
                if (c == ';') {
                    extending = false;
                    extend.Append(pusher);
                    pusher = "";
                    if (outside)
                        retType = extend.ToString();
                    else
                        funcArgs.Add(extend.ToString());
                    extend.Clear();
                } else
                    extend.Append(c);
                continue;
            }
            switch (c) {
                case ')':
                outside = true;
                break;
                case 'L':
                extending = true;
                break;
                case '[':
                pusher += "[]";
                break;
                case 'B':
                if (outside)
                    retType = "byte" + pusher;
                else
                    funcArgs.Add("byte" + pusher);
                pusher = "";
                break;
                case 'C':
                if (outside)
                    retType = "char" + pusher;
                else
                    funcArgs.Add("char" + pusher);
                pusher = "";
                break;
                case 'D':
                if (outside)
                    retType = "double" + pusher;
                else
                    funcArgs.Add("double" + pusher);
                pusher = "";
                break;
                case 'F':
                if (outside)
                    retType = "float" + pusher;
                else
                    funcArgs.Add("float" + pusher);
                pusher = "";
                break;
                case 'I':
                if (outside)
                    retType = "int" + pusher;
                else
                    funcArgs.Add("int" + pusher);
                pusher = "";
                break;
                case 'J':
                if (outside)
                    retType = "long" + pusher;
                else
                    funcArgs.Add("long" + pusher);
                pusher = "";
                break;
                case 'S':
                if (outside)
                    retType = "short" + pusher;
                else
                    funcArgs.Add("short" + pusher);
                pusher = "";
                break;
                case 'Z':
                if (outside)
                    retType = "boolean" + pusher;
                else
                    funcArgs.Add("boolean" + pusher);
                pusher = "";
                break;
                case 'V':
                if (outside)
                    retType = "void" + pusher;
                else
                    funcArgs.Add("void" + pusher);
                pusher = "";
                break;
            }
        }
        Console.WriteLine("Initial parse completed successfully, building output...");
        return retType + " method(" + string.Join(", ", funcArgs.Apply((string Type, int Index) => Type + " arg" + Index)) + ")";
    }
}