CREATE PROCEDURE SBO_EXX_PM_BCP (
docEntry int, glaccount nvarchar(15)
)

AS
factoring nvarchar(1);
BEGIN
-- Llenado de variables
SELECT "U_EXC_FCTRNG" INTO factoring FROM DSC1 WHERE "GLAccount"=:glaccount and ifnull("U_EXM_PMASIVO",'') = 'Y';

IF :factoring='N' THEN

SELECT
A0."001-001 (1)"
||A0."002-002(1)"
||A0."003-003(1)"
||A0."004-004(1)"
||A0."005-024(20)"
||A0."025-026(2)"
||A0."027-041(15)"
||A0."042-049(8)"
||A0."050-069(20)"
||A0."070-084(15)"
||A0."085-090(6)"
||A0."091-091(1)"
||A0."092-106(15)"
||A0."107-107(1)"
||'|'
AS "Resultado"

FROM (
	----- CABECERA -----
	SELECT
	'#' AS "001-001 (1)",
	'1' AS "002-002(1)",
	'P' AS "003-003(1)",
	LEFT(ifnull(T2."UsrNumber2",''),1) AS "004-004(1)",
	LEFT(
		LEFT(REPLACE(T2."Account",'-',''),3)
		||RIGHT(replicate('0',8)||LEFT(RIGHT(REPLACE(T2."Account",'-',''),LENGTH(REPLACE(T2."Account",'-',''))-3),LENGTH(RIGHT(REPLACE(T2."Account",'-',''),LENGTH(REPLACE(T2."Account",'-',''))-3))-3),8)
		||RIGHT(REPLACE(T2."Account",'-',''),3)
		||replicate(' ',20)
		,20) AS "005-024(20)",
	CASE T2."UsrNumber1" WHEN 'SOL' THEN 'S/' WHEN 'USD' THEN 'US' ELSE '' END AS "025-026(2)",
	RIGHT(replicate('0',15)||REPLACE(REPLACE(CAST(CAST(SUM(T1."U_EXP_IMPORTE") AS DECIMAL(18,2)) AS NVARCHAR(20)),',',''),'.',''),15) AS "027-041(15)", -- SUMA DETALLE
	TO_NVARCHAR(T0."U_EXP_FECHAPAGO",'DDMMYYYY') AS "042-049(8)",
	replicate(' ',20) AS "050-069(20)",
	RIGHT(replicate('0',15)||
		CAST(
		CAST(RIGHT(replicate('0',8)||LEFT(RIGHT(REPLACE(T2."Account",'-',''),LENGTH(REPLACE(T2."Account",'-',''))-3),LENGTH(RIGHT(REPLACE(T2."Account",'-',''),LENGTH(REPLACE(T2."Account",'-',''))-3))-3),8)
			||RIGHT(REPLACE(T2."Account",'-',''),3) AS BIGINT)
		+
		SUM(CAST(RIGHT(
			RIGHT(replicate('0',8)||LEFT(RIGHT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),LENGTH(REPLACE(T1."U_EXP_NROCTAPROV",'-',''))-3),LENGTH(RIGHT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),LENGTH(REPLACE(T1."U_EXP_NROCTAPROV",'-',''))-3))-3),8)
			||RIGHT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),3)
			,15) AS BIGINT))
		AS NVARCHAR(100)),15)
		AS "070-084(15)", -- checksum
	RIGHT(replicate('0',6)||COUNT(T1."DocEntry"),6) AS "085-090(6)",
	'0' AS "091-091(1)",
	replicate(' ',15) AS "092-106(15)",
	'0' AS "107-107(1)" -- Nota de cargo 0:no 1:si
	
	FROM "@EXP_OPMP" T0
	INNER JOIN "@EXP_PMP1" T1 ON T0."DocEntry"=T1."DocEntry"
	INNER JOIN DSC1 T2 ON T1."U_EXP_CODCTABANCO"=T2."GLAccount"
	WHERE T0."DocEntry"= :docEntry
	AND T1."U_EXP_CODBANCO"='002' AND T1."U_EXP_CODCTABANCO"=:glaccount
	AND IFNULL(T2."U_EXM_PMASIVO",'') = 'Y'
	AND T1."U_EXP_MEDIODEPAGO" IN ('TB','CG') AND T1."U_EXP_SLC_RETENCION"='N'
	--AND T1."U_EXP_CARDCODE"='P20603816898'
	AND ((T1."U_EXP_MEDIODEPAGO"='TB' AND IFNULL(T1."U_EXP_NROCTAPROV",'')!='') OR (T1."U_EXP_MEDIODEPAGO"='CG'))
	GROUP BY T2."UsrNumber1",T2."UsrNumber2",T2."Account",T0."U_EXP_FECHAPAGO"
	----- FIN CABECERA -----
) A0

UNION ALL

SELECT
A1."001-001(1)"
||A1."002-002(1)"
||A1."003-003(1)"
||A1."004-023(20)"
||A1."024-063(40)"
||A1."064-065(2)"
||A1."066-080(15)"
||A1."081-083(3)"
||A1."084-095(12)"
||A1."096-096(1)"
||A1."097-106(10)"
||A1."107-107(1)"
||A1."108-147(40)"
||A1."148-148(1)"
||A1."149-149(1)"
||A1."150-150(1)"
||A1."151-190(40)"
||A1."191-210(20)"
||A1."211-230(20)"
||A1."231-250(20)"
||A1."251-290(40)"
||'|'
AS "Resultado"

FROM (
	----- DETALLE -----
	SELECT
	' ' AS "001-001(1)",
	CASE T1."U_EXP_MEDIODEPAGO"
		WHEN 'TB' THEN '2'
		WHEN 'CG' THEN '0'
		ELSE ' ' END AS "002-002(1)",
	CASE T1."U_EXP_MEDIODEPAGO"
		WHEN 'CG' THEN 'C'
		ELSE
			CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
				WHEN '002' THEN (SELECT ifnull("UsrNumber2",'') FROM OCRB WHERE "CardCode"=T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV" AND "BankCode"='002')
				ELSE 'B' END
			END AS "003-003(1)",
	CASE T1."U_EXP_MEDIODEPAGO"
		WHEN 'CG' THEN replicate(' ',20)
		ELSE
			CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV") WHEN '002' THEN
			LEFT(LEFT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),3)
				||RIGHT(replicate('0',8)||LEFT(RIGHT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),LENGTH(REPLACE(T1."U_EXP_NROCTAPROV",'-',''))-3),LENGTH(RIGHT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),LENGTH(REPLACE(T1."U_EXP_NROCTAPROV",'-',''))-3))-3),8)
				||RIGHT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),3)
				||replicate(' ',20),20)
			ELSE LEFT(REPLACE(T1."U_EXP_NROCTAPROV",'-','')||replicate(' ',20),20) END END AS "004-023(20)",
	LEFT(T1."U_EXP_CARDNAME"||replicate(' ',40),40) AS "024-063(40)",
	CASE T1."U_EXP_MONEDA" WHEN 'SOL' THEN 'S/' WHEN 'USD' THEN 'US' ELSE '' END AS "064-065(2)",
	RIGHT(replicate('0',15)||REPLACE(REPLACE(CAST(CAST(T1."U_EXP_IMPORTE" AS DECIMAL(18,2)) AS NVARCHAR(20)),',',''),'.',''),15) AS "066-080(15)",
	CASE T3."U_EXX_TIPODOCU"
		WHEN '1' THEN 'DNI'
		WHEN '4' THEN 'CE '
		WHEN '6' THEN 'RUC'
		WHEN '7' THEN 'PAS'
		ELSE '' END AS "081-083(3)",
	LEFT(T3."LicTradNum"||replicate(' ',12),12) AS "084-095(12)",
	CASE T1."U_EXP_MEDIODEPAGO"
		WHEN 'TB' THEN
			CASE T1."U_EXP_TIPODOC"
			WHEN '18' THEN 'F'
			WHEN '14' THEN 'N'
			ELSE 'D' END
		WHEN 'CG' THEN 'F'
		ELSE ' ' END AS "096-096(1)",
	--RIGHT(replicate('0',10)||T1."U_EXP_DOCENTRYDOC",10) AS "097-106(10)",
	RIGHT(replicate('0',10)||(REPLACE(IFNULL(T4."NumAtCard", IFNULL(T5."NumAtCard",T1."U_EXP_DOCENTRYDOC")), '-', '')),10) AS "097-106(10)",
	
	--REPLACE(IFNULL(T4."NumAtCard", IFNULL(T5."NumAtCard",T1."U_EXP_DOCENTRYDOC")), '-', '')
	
	'1' AS "107-107(1)", --1:ABONO SIMPLE , 2: ABONO MULTIPLE | Gustavo comenta siempre 1
	replicate(' ',40) AS "108-147(40)",
	'0' AS "148-148(1)",
	'0' AS "149-149(1)",
	'1' AS "150-150(1)", --0: NO, 1:SI | desea validar IDC vs Cuenta (Ejm SIEMPRE SI)
	LEFT(IFNULL(T3."Address",'')||replicate(' ',40),40) AS "151-190(40)",
	LEFT(IFNULL(' '||T3."ZipCode",'')||replicate(' ',20),20) AS "191-210(20)",
	LEFT(IFNULL(' '||T3."County",'')||replicate(' ',20),20) AS "211-230(20)",
	LEFT(IFNULL((SELECT "Name" FROM OCST WHERE "Country"='PE' AND "Code"=T3."State1"),'')||replicate(' ',20),20) AS "231-250(20)",
	replicate(' ',40) AS "251-290(40)"
	
	FROM "@EXP_OPMP" T0
	INNER JOIN "@EXP_PMP1" T1 ON T0."DocEntry"=T1."DocEntry"
	INNER JOIN DSC1 T2 ON T1."U_EXP_CODCTABANCO"=T2."GLAccount"
	INNER JOIN OCRD T3 ON T1."U_EXP_CARDCODE"=T3."CardCode"
	LEFT JOIN OPCH T4 ON T1."U_EXP_DOCENTRYDOC"=T4."DocEntry" AND T1."U_EXP_TIPODOC"=T4."ObjType"
	LEFT JOIN ODPO T5 ON T1."U_EXP_DOCENTRYDOC"=T5."DocEntry" AND T1."U_EXP_TIPODOC"=T5."ObjType"
	WHERE T0."DocEntry"=:docEntry
	AND T1."U_EXP_CODBANCO"='002' AND T1."U_EXP_CODCTABANCO"=:glaccount
	AND IFNULL(T2."U_EXM_PMASIVO",'') = 'Y'
	AND T1."U_EXP_MEDIODEPAGO" IN ('TB','CG') AND T1."U_EXP_SLC_RETENCION"='N'
	AND ((T1."U_EXP_MEDIODEPAGO"='TB' AND IFNULL(T1."U_EXP_NROCTAPROV",'')!='') OR (T1."U_EXP_MEDIODEPAGO"='CG'))
	--AND T1."U_EXP_CARDCODE"='P20603816898'
	----- FIN -----
) A1;

END IF;

END;