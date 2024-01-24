parser grammar UtilityTypesParser;

options {
	tokenVocab = UtilityTypesLexer;
}
    
has_props_verb :
    PICK_KEYWORD |
    OMIT_KEYWORD ;

has_props : has_props_verb LT symbol_or_selector comma_or property_name (comma_or property_name)* GT ;

has_type_verb : 
    IMPORT_KEYWORD |
    NOTNULL_KEYWORD |
    NULLABLE_KEYWORD |
    OPTIONAL_KEYWORD |
    READONLY_KEYWORD |
    REQUIRED_KEYWORD ;

has_type : has_type_verb LT symbol_or_selector GT ;

has_types_verb : 
    INTERSECTION_KEYWORD |
    INTERSECT_KEYWORD |
    UNION_KEYWORD ;

has_types : has_types_verb LT symbol_or_selector (comma_or symbol_or_selector)* GT ;

selector : (has_props | has_type | has_types ) SEMICOLON? ;
    
symbol_or_selector: symbol | selector;
symbol: (qualified_id) |
    (qualified_id LT symbol (COMMA symbol)* GT) |
    (qualified_id LT symbol (COMMA symbol)* GT) ;

property_name   : quoted_or_unquoted_id;

qualified_id            : ID (DOT ID)*;
quoted_id               : (DQ unquoted_id DQ) | (SQ unquoted_id SQ);
unquoted_id             : ID;
quoted_or_unquoted_id   : unquoted_id | quoted_id;
comma_or                : '|' | ',';
