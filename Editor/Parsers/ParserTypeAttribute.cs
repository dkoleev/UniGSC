using System;

namespace Yogi.GoogleSheetsConfig.Editor.Parsers {
    public class ParserTypeAttribute : Attribute {
        public string ParserType { get; private set; }

        public ParserTypeAttribute(string parserType) {
            ParserType = parserType;
        }
    }
}