lexer grammar UtilityTypesLexer;

// keywords
NAMESPACE_KEYWORD       : 'namespace';
TYPE_KEYWORD            : 'type';
PICK_KEYWORD            : 'Pick';
OMIT_KEYWORD            : 'Omit';
UNION_KEYWORD           : 'Union';
INTERSECTION_KEYWORD    : 'Intersection';
INTERSECT_KEYWORD       : 'Intersect';
OPTIONAL_KEYWORD        : 'Optional';
REQUIRED_KEYWORD        : 'Required';
READONLY_KEYWORD        : 'Readonly';
NOTNULL_KEYWORD         : 'NotNull';
NULLABLE_KEYWORD        : 'Nullable';
IMPORT_KEYWORD          : 'Import';

fragment InputCharacter: ~[\r\n\u0085\u2028\u2029];
fragment Digit: [0-9];
fragment Lowercase: [a-z];
fragment Uppercase: [A-Z];
fragment DoubleQuote: '"';
fragment SingleQuote: '\'';
fragment AnyQuote: DoubleQuote | SingleQuote;
fragment Semicolon: ';';

ID: [a-zA-Z_][a-zA-Z_0-9]* ;
WS: [ \t\n\r\f]+ -> skip ;

DOT: '.';
LT : '<';
GT : '>';
COMMA : ',';
BITWISE_OR : '|';
SEMICOLON : Semicolon;
DQ: DoubleQuote;
SQ: SingleQuote;
