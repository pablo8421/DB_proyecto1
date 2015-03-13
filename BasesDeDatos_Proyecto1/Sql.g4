grammar Sql;

options {
language=Csharp;
}

WS : [ \t\r\n]+ -> skip ;
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
fragment FLOAT : '-'? NUMBER+ '.' NUMBER+;
fragment INT : '-'? NUMBER+;
fragment IDENTIFICADOR: ID ('.' ID)?;
fragment ID : LETTER (LETTER|NUMBER)*; 
fragment STRING: '\'' .*? '\'';
fragment VALUE : (INT | FLOAT | STRING); 


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

multi_constraint_completo: multi_constraint_completo ',' constraint_completo 
						 | constraint_completo;

constraint_completo: CONSTRAINT constraint;

columnas: ID tipo;

multi_columnas: multi_columnas ',' columnas 
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
				| exp;

exp: (ID | INT | FLOAT | STRING); //Cree Pablo que tambien pondriamos identificador

accion: RENAME TO ID
	  | ADD COLUMN ID tipo  (multi_constraint_completo)?
	  | ADD constraint_completo
	  | DROP COLUMN ID
	  | DROP CONSTRAINT ID;

multi_accion: multi_accion ',' accion 
			| accion;

alter_table: ALTER TABLE ID multi_accion;

votar_table: DROP TABLE ID;

show_tables: SHOW TABLES;

show_columns: SHOW COLUMNS FROM ID;

//-------------------------------------

insert : INSERT INTO ID ('(' id_completo ')')? VALUES '(' valor_completo ')';

id_completo : id_completo ',' ID
				 | ID;

valor_completo : valor_completo ',' VALUE
				 | VALUE;

update : UPDATE ID SET asignacion (WHERE multi_exp)?;

asignacion : asignacion ',' ID '=' VALUE
			| ID '=' VALUE;

delete : DELETE FROM ID (WHERE multi_exp)?;

select :  SELECT ('*' | id_completo) FROM id_completo (WHERE multi_exp)? (ORDER BY id_completo_order)?

id_completo_order : id_completo_order ',' ID  (ASC|DESC)?
				 | ID  (ASC|DESC)?;
