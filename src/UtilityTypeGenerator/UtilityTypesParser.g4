parser grammar UtilityTypesParser;

options {
	tokenVocab = UtilityTypesLexer;
}

import_selector : IMPORT_KEYWORD LT selector GT SEMICOLON?;
required        : REQUIRED_KEYWORD LT selector GT SEMICOLON?;
optional        : OPTIONAL_KEYWORD LT selector GT SEMICOLON?;
readonly        : READONLY_KEYWORD LT selector GT SEMICOLON?;
notnull         : NOTNULL_KEYWORD LT selector GT SEMICOLON?;
nullable        : NULLABLE_KEYWORD LT selector GT SEMICOLON?;
pick            : PICK_KEYWORD LT selector COMMA property_name (BITWISE_OR property_name)* GT SEMICOLON?;
omit            : OMIT_KEYWORD LT selector COMMA property_name (BITWISE_OR property_name)* GT SEMICOLON?;
union           : UNION_KEYWORD LT selector (COMMA selector)+ GT SEMICOLON?;
intersection    : (INTERSECTION_KEYWORD | INTERSECT_KEYWORD) LT selector (COMMA selector)+ GT SEMICOLON?;
property_name   : ID;

utility: required | import_selector | optional | readonly | notnull | nullable | pick | omit | union | intersection;
selector: type_name | utility;
type_name: qualified_id |
    (qualified_id LT type_name (COMMA type_name)* GT) |
    (qualified_id LT type_name (COMMA type_name)* GT) ;

qualified_id: ID (DOT ID)*;
quoted_id: (DQ ID DQ) | (SQ ID SQ);
quoted_or_unquoted_id: ID | quoted_id;
