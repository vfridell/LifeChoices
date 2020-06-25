grammar RuleTable;

/*
 * Parser Rules
 */

tableDef			: stmt+;
stmt : 
		(states
	|	neighborhood 
	|	sym 
	|	varline 
	|	transitionline 
	) (NEWLINE | EOF)
	| NEWLINE
	| NEWLINE EOF
	 ;
states				: NSTATES_KW ':' NUM;
neighborhood		: NHOOD_KW ':' NHOOD_NAME;
sym					: SYM_KW ':' SYM_NAME;
varline				: 'var' WORD '=' statelist ; 
statelist			: '{' listitem ( ',' listitem)* '}';
listitem			: NUM;
transitionline		: transitionitem ( ',' transitionitem)* ;
transitionitem		: (WORD | NUM);

/*
 * Lexer Rules
 */
fragment LOWERCASE  :	[a-z] ;
fragment UPPERCASE  :	[A-Z] ;

NSTATES_KW			:	'n_states';
NHOOD_KW			:	'neighborhood';
SYM_KW				:	'symmetries';

NHOOD_NAME			:	('vonNeumann' | 'Moore');
SYM_NAME			:	('none'| 'rotate4'| 'rotate8'| 'rotate4reflect'| 'rotate8reflect'| 'reflect_horizontal'| 'permute');
NUM					:	[0-9]+;
WORD                :	(LOWERCASE | UPPERCASE | '_')+ ;
WS					:	(' ' | '\t')+ -> channel(HIDDEN); 
NEWLINE             :	('\r'? '\n' | '\r')+ ;
COMMENT				:	'#' ~[\r\n]* NEWLINE -> channel(HIDDEN);
