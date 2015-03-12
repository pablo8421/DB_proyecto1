grammar Sql;

options {
language=Csharp;
}

CREATE : 'CREATE';
DATABASE : 'DATABASE';
ALTER: 'ALTER';
RENAME: 'RENAME';
TO: 'TO';
DROP:'DROP';
SHOW:'SHOW';
USE:'USE';


TABLE:'TABLE';
CONSTRAINT: 'CONSTRAINT';
PRIMARY:'PRIMARY';
FOREIGN:'FOREIGN';
KEY:'KEY';
REFERENCES: 'REFERENCES';

ADD: 'ADD';
COLUMN: 'COLUMN';
TABLES:'TABLES';
FROM: 'FROM';

fragment LETTER: [a-z] | [A-Z];
fragment NUMBER: [0-9];
fragment ID : LETTER (LETTER|NUMBER)* ;   //En algun momento abria que tomar en cuenta columnas de tablas
fragment STRING: '\'' ID '\''; //Aca iria todo no solo ID

crear_BD: CREATE DATABASE ID;

renombrar_BD: ALTER DATABASE ID RENAME TO ID;

votar_BD: DROP DATABASE ID;

mostrar_BD: SHOW DATABASE;

usar_BD: USE DATABASE ID;

tipo: 'INT'
	| 'FLOAT'
	| 'DATE'
	| 'CHAR' '(' NUMBER+ ')';

multi_id: ID ',' multi_id
		| ID;

constraint: ID PRIMARY KEY '(' multi_id ')'
		  | ID FOREIGN KEY '(' multi_id ')' REFERENCES ID '(' multi_id ')'
		  | ID CHECK '(' multi_exp ')';

multi_constraint_completo: constraint_completo ',' multi_constraint_completo
						 | constraint_completo;

constraint_completo: CONSTRAINT constraint;

columnas: ID tipo;

multi_columnas: columnas ',' multi_columnas
			  |  columnas;

crear_tabla: CREATE TABLE ID '(' multi_columnas (multi_constraint_completo)?')';

multi_exp : multi_exp 'OR' and_expression
				| and_expression;
				
and_expression : and_expression 'AND' difEq_expression
				|  difEq_expression;

difEq_expression : difEq_expression ('<>'|'=') mayMin_expression
				| mayMin_expression;

mayMin_expression : mayMin_expression ('>='|'<='|'>'|'<') sum_expression
				| neg_expression;

neg_expression : 'NOT' neg_expression
				| paren_expression;

paren_expression : '(' multi_exp ')';

exp: (ID | NUMBER+ | STRING) exp_relacionales (ID | NUMBER+ | STRING);

accion: RENAME TO ID
	  | ADD COLUMN ID tipo  (multi_constraint_completo)?
	  | ADD constraint_completo
	  | DROP COLUMN ID
	  | DROP CONSTRAINT ID; //No se si es id aca

alter_table: ALTER TABLE ID accion;

votar_table: DROP TABLE ID;

show_tables: SHOW TABLES;

show_columns: SHOW COLUMNS FROM ID;

WS : [ \t\r\n]+ -> skip ; // skip spaces, tabs, newlines



//EL CAMBIO