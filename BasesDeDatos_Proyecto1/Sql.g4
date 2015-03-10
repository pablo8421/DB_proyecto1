grammar Sql;

options {
language=Csharp;
}

CREATE : 'CREATE'  | 'create';
DATABASE : 'DATABASE' | 'database';
ALTER: 'ALTER' | 'alter';
RENAME: 'RENAME' | 'rename';
TO: 'TO' | 'to';
DROP:'DROP' | 'drop';
SHOW:'SHOW' | 'show';
USE:'USE' | 'use';


TABLE:'TABLE' | 'table';
CONSTRAINT: 'CONSTRAINT' | 'constraint';
PRIMARY:'PRIMARY' | 'primary';
FOREIGN:'FOREIGN' | 'foreign';
KEY:'KEY' | 'key';
REFERENCES: 'REFERENCES' | 'references';

ADD: 'ADD' | 'add';
COLUMN: 'COLUMN' | 'column';
TABLES:'TABLES' | 'tables';
FROM: 'FROM' | 'from';

fragment LETTER: [a-z] | [A-Z];
fragment NUMBER: [0-9];
fragment ID : LETTER (LETTER|NUMBER)* ;   //EN algun momento abria que tomar en cuenta columnas de tablas
fragment STRING: '\'' ID '\''; //Aca iria todo no solo ID

crear_BD: CREATE DATABASE ID;

renombrar_BD: ALTER DATABASE ID RENAME TO ID;

votar_BD: DROP DATABASE ID;

mostrar_BD: SHOW DATABASE;

usar_BD: USE DATABASE ID;

tipo: 'int'
	| 'float'
	| 'date'
	| 'char'
	| 'char' '(' NUMBER+ ')';

// Aca me quede

multi_id: ID ',' multi_id
		| ID;

constraint: ID tipo? PRIMARY KEY '(' multi_id ')'
		  | ID tipo? FOREIGN KEY '(' multi_id ')' REFERENCES ID '(' multi_id ')'
		  | CHECK '(' multi_exp ')';

multi_constraint: constraint ',' multi_constraint
				| constraint;

columnas: ID tipo;

multi_columnas: columnas ',' multi_columnas
			  |  columnas;

crear_tabla: CREATE TABLE ID '(' multi_columnas (CONSTRAINT multi_constraint)?')';

multi_exp: exp exp_logico multi_exp
		 | exp; 

exp: (ID | NUMBER+ | STRING) exp_relacionales (ID | NUMBER+ | STRING);

exp_logico: 'AND'
		  | 'OR'
		  | 'NOT';

exp_relacionales: '<'
				| '>'
				| '<='
				| '>='
				| '<>'
				| '=';

accion: RENAME TO ID
	  | ADD COLUMN ID tipo  multi_constraint
	  | ADD CONSTRAINT constraint
	  | DROP COLUMN ID
	  | DROP CONSTRAINT ID; //No se si es id aca

alter_table: ALTER TABLE ID accion;

votar_table: DROP TABLE ID;

show_tables: SHOW TABLES;

show_columns: SHOW COLUMNS FROM ID;

WS : [ \t\r\n]+ -> skip ; // skip spaces, tabs, newlines