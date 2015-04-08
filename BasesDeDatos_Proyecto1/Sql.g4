/*
Pablo Sánchez, 12148
César Guerra, 12593
Sección 10
Gramática del proyecto
*/

grammar Sql;

options {
language=Csharp;
}

CREATE : 'CREATE';
DATABASE : 'DATABASE';
DATABASES : 'DATABASES';
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
COLUMNS: 'COLUMNS';
TABLES:'TABLES';
FROM: 'FROM';
CHECK: 'CHECK';
NULL: 'NULL';

//---------------------------------

INSERT : 'INSERT';
INTO : 'INTO';
VALUES : 'VALUES';
UPDATE : 'UPDATE';
SET : 'SET';
WHERE : 'WHERE';
DELETE : 'DELETE';
SELECT : 'SELECT';
ORDER : 'ORDER';
BY : 'BY';
ASC : 'ASC';
DESC : 'DESC';


fragment LETTER: [a-z] | [A-Z];
fragment NUMBER: [0-9];

FLOAT : '-'? NUMBER+ '.' NUMBER+;
INT : '-'? NUMBER+;
STRING: '\'' .*? '\'';

ID : LETTER (LETTER|NUMBER|'!'|'#'|'$'|'%'|'&'|'+'|'-'|'@'|'['|']'|'^'|'_'|'`'|'{'|'}'|'~')*; 
IDENTIFICADOR: ID '.' ID; 

//--------------------------------------

full_query: (queries+= query ';')*;

query: crear_BD			
	 | renombrar_BD		
	 | botar_BD			
	 | mostrar_BD		
	 | usar_BD			
	 | crear_tabla		
	 | alter_table		
	 | botar_table		
	 | show_tables		
	 | show_columns		
	 | insert			
	 | update			
	 | delete			
	 | select;			

crear_BD: CREATE DATABASE ID;

renombrar_BD: ALTER DATABASE ID RENAME TO ID;

botar_BD: DROP DATABASE ID;

mostrar_BD: SHOW DATABASES;

usar_BD: USE DATABASE ID;

tipo: 'INT'
	| 'FLOAT'
	| 'DATE'
	| 'CHAR' '(' INT ')';

multi_id: ID ',' multi_id
		| ID;

constraint: ID PRIMARY KEY '(' multi_id ')' #constrain_pk
		  | ID FOREIGN KEY '(' multi_id ')' REFERENCES ID '(' multi_id ')' #constrain_fk
		  | ID CHECK '(' multi_exp ')' #constrain_check;

multi_constraint_completo: multi_constraint_completo ',' constraint_completo 
						 | constraint_completo;

constraint_completo: CONSTRAINT constraint;

columnas: ID tipo;

multi_columnas: multi_columnas ',' columnas 
			  |  columnas;

crear_tabla: CREATE TABLE ID '(' multi_columnas (',' multi_constraint_completo)?')';

multi_exp : multi_exp 'OR' and_expression
				| and_expression;
				
and_expression : and_expression 'AND' difEq_expression
				|  difEq_expression;

difEq_expression : difEq_expression ('<>'|'=') mayMin_expression
				| mayMin_expression;

mayMin_expression : mayMin_expression ('>='|'<='|'>'|'<') neg_expression
				| neg_expression;

neg_expression : 'NOT' neg_expression
				| paren_expression;

paren_expression : '(' multi_exp ')'
				| exp;

exp: (IDENTIFICADOR | ID)	#exp_Ident
   | INT					#exp_Int
   | FLOAT					#exp_Float
   | STRING					#exp_String
   | NULL					#exp_Null;

accion: RENAME TO ID #accion_rename
	  | ADD COLUMN ID tipo  (multi_constraint_completo)? #accion_addColumn
	  | ADD constraint_completo #accion_addConstraint
	  | DROP COLUMN ID #accion_DropColumn
	  | DROP CONSTRAINT ID #accion_DropConstraint;

multi_accion: multi_accion ',' accion 
			| accion;

alter_table: ALTER TABLE ID multi_accion;

botar_table: DROP TABLE ID;

show_tables: SHOW TABLES;

show_columns: SHOW COLUMNS FROM ID;

//-------------------------------------

insert : INSERT INTO ID ('(' id_completo ')')? VALUES '(' valor_completo ')';

id_completo : id_completo ',' ID
				 | ID;

valor_completo : valor_completo ',' (INT | FLOAT | STRING | NULL)
				 | (INT | FLOAT | STRING | NULL);

update : UPDATE ID SET asignacion (WHERE multi_exp)?;

asignacion : asignacion ',' ID '=' (INT | FLOAT | STRING | NULL)
			| ID '=' (INT | FLOAT | STRING | NULL);

delete : DELETE FROM ID (WHERE multi_exp)?;

identificador_completo : identificador_completo ',' (IDENTIFICADOR | ID)
				 | (IDENTIFICADOR | ID);

id_tablas: id_tablas ',' ID
		 | ID; 

select :  SELECT ('*' | identificador_completo) 
		  FROM id_tablas select_where select_orderBy;

select_where: (WHERE multi_exp)?;

select_orderBy: (ORDER BY id_completo_order)?;

id_completo_order : id_completo_order ',' (IDENTIFICADOR | ID)  (ASC|DESC)? #id_completo_orderVarios
				 | (IDENTIFICADOR | ID) (ASC|DESC)?							#id_completo_orderSolo;

WS : [ \t\r\n]+ -> skip ;