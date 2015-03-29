grammar Sql;

options {
language=Csharp;
}

WS : [ \t\r\n]+ -> skip ;
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

NUM : NUMBER+;
FLOAT : '-'? NUMBER+ '.' NUMBER+;
INT : '-'? NUMBER+;
STRING: '\'' .*? '\'';

ID : LETTER (LETTER|NUMBER)*; 
IDENTIFICADOR: ID '.' ID;
VALUE : (INT | FLOAT | STRING); 


full_query: (queries+= query ';')*;

query: crear_BD			//CHECK
	 | renombrar_BD		//CHECK
	 | botar_BD			//CHECK
	 | mostrar_BD		//CHECK
	 | usar_BD			//CHECK
	 | crear_tabla		//CHECK
	 | alter_table		//CHECK
	 | botar_table		//CHECK
	 | show_tables		//CHECK
	 | show_columns		//CHECK
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
	| 'CHAR' '(' NUM ')';

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

crear_tabla: CREATE TABLE ID '(' multi_columnas (multi_constraint_completo)?')';

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
   | STRING					#exp_String;

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

valor_completo : valor_completo ',' VALUE
				 | VALUE;

update : UPDATE ID SET asignacion (WHERE multi_exp)?;

asignacion : asignacion ',' ID '=' VALUE
			| ID '=' VALUE;

delete : DELETE FROM ID (WHERE multi_exp)?;

identificador_completo : identificador_completo ',' (IDENTIFICADOR | ID)
				 | (IDENTIFICADOR | ID);

select :  SELECT ('*' | identificador_completo) 
		  FROM id_completo select_where select_orderBy;

select_where: (WHERE multi_exp)?;

select_orderBy: (ORDER BY id_completo_order)?;

id_completo_order : id_completo_order ',' (IDENTIFICADOR | ID)  (ASC|DESC)?
				 | (IDENTIFICADOR | ID) (ASC|DESC)?;