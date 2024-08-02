namespace SwiftMsgAssignment
{
    public static class StringExtension
    {
        //returns a substring divided by two other set strings
        public static string SubstringInbetween(this string source, string start, string end)
        {
            int startidx, endidx;

            startidx = source.IndexOf(start) + start.Length;
            if (startidx > start.Length - 1) endidx = source.IndexOf(end, startidx);
            else return null;
            if (endidx > -1) return source.Substring(startidx, endidx - startidx);
            else return null;
        }
    }
}
