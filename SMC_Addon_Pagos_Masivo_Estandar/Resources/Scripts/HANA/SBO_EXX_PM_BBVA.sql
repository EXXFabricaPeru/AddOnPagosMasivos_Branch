CREATE PROCEDURE SBO_EXX_PM_BBVA(
docEntry int, 
codSucursal int,
glaccount nvarchar(15)
)

AS
factoring nvarchar(1);
bankCode nvarchar(5);
BEGIN
-- Llenado de variables
SELECT max("U_EXC_FCTRNG") INTO factoring FROM DSC1 WHERE "GLAccount"=:glaccount and "Branch" = :codSucursal and ifnull("U_EXM_PMASIVO",'') = 'Y';
SELECT '011' INTO bankCode FROM DUMMY;

IF ifnull(:factoring,'N') = 'N' THEN

SELECT
A0."001-003 (3)" --Tipo de Registro
||A0."004-023(20)"--Cuenta de Cargo
||A0."024-026(3)"--Moneda de Cuenta de Cargo
||A0."027-041(15)"--Importe a Cargar
||A0."042-042(1)"--Tipo de Proceso A: Inmediato H:Hora F:Fecha
||A0."043-050(8)"--Fecha de Proceso - Opcional si Tipo es F
||A0."051-051(1)"--Hora Proceso - Opcional si tipo es H - B:11:00am C:3:00pm D:7:00pm
||A0."052-076(25)"--REFERENCIA opcional

||A0."077-082(6)"--TOtal de Registros
||A0."083-083(1)" --Validación de pertencia S: Valida, si hay error rechaza abono N: Novalida

||A0."084-098(15)" --Valor de control -Banco
||A0."099-101(3)" --Indicador de proceso-Banco
||A0."102-131(30)" --Descripción --Banco
||A0."132-146(15)" --IMporte máximo por registro-opcional
||A0."147-151(5)" --Filler-Banco -Banco
||'|'
AS "Resultado"

FROM (
	----- CABECERA -----
	SELECT
	'750' AS "001-003 (3)",--TIPO REGISTRO
	case 
		when length(replace(T2."Account",'-','')) = 20 then replace(T2."Account",'-','')
		when length(replace(T2."Account",'-','')) = 18 then 
			left(replace(T2."Account",'-',''),8) ||'00'||right(replace(T2."Account",'-',''),length(replace(T2."Account",'-',''))-8) 
		else lpad('',20,'0') end AS "004-023(20)",--CUENTA CARGO
	CASE T2."UsrNumber4" WHEN 'SOL' THEN 'PEN' WHEN 'USD' THEN 'USD' ELSE '' END AS "024-026(3)",--MONEDA
	RIGHT(replicate('0',15)||REPLACE(REPLACE(CAST(CAST(SUM(ROUND(T1."U_EXP_IMPORTE",2)) AS DECIMAL(18,2)) AS NVARCHAR(20)),',',''),'.',''),15) AS "027-041(15)", -- IMPORTE A CARGAR
	'A'  as "042-042(1)",--Tipo de Proceso A: Inmediato H:Hora F:Fecha
	replicate(' ',8) AS "043-050(8)", --Fecha de Proceso - Opcional si Tipo es F
	' ' as "051-051(1)", -- --Hora Proceso - Opcional si tipo es H - B:11:00am C:3:00pm D:7:00pm
	replicate(' ',25) AS "052-076(25)",--Referencia
	LPAD(SUM(1),6,'0') "077-082(6)",--TOtal de Registros

	'S' as "083-083(1)",  --Validación de pertencia S: Valida, si hay error rechaza abono N: Novalida

	replicate(' ',15) as "084-098(15)", --Valor de control - banco
	replicate(' ',3)  as "099-101(3)", --Indicador de proceso - banco
	replicate(' ',30) as "102-131(30)", --Descripción - banco
	replicate(' ',15) as "132-146(15)", --IMporte máximo por registro-opcional
	replicate(' ',5) as "147-151(5)", --Filler - banco

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
	INNER JOIN "@EXP_PMP1" T1 ON T0."DocEntry" = T1."DocEntry"
	INNER JOIN DSC1 T2 ON T1."U_EXP_CODCTABANCO" = T2."GLAccount" and T2."Branch" = T1."U_EXP_COD_SUCURSAL"
	WHERE 
	T0."DocEntry"= :docEntry
	 
	AND T1."U_EXP_CODBANCO"=:bankCode 
	AND T1."U_EXP_CODCTABANCO"	=	:glaccount
	AND T1."U_EXP_COD_SUCURSAL"	=	:codSucursal
	AND IFNULL(T2."U_EXM_PMASIVO",'') = 'Y'
	AND T1."U_EXP_MEDIODEPAGO" IN ('TB','CG') AND T1."U_EXP_SLC_RETENCION"='N'
	--AND T1."U_EXP_CARDCODE"='P20603816898'
	AND ((T1."U_EXP_MEDIODEPAGO"='TB' AND IFNULL(T1."U_EXP_NROCTAPROV",'')!='') OR (T1."U_EXP_MEDIODEPAGO"='CG'))
	GROUP BY T2."UsrNumber4",T2."UsrNumber2",T2."Account"--,T0."U_EXP_FECHAPAGO"
	----- FIN CABECERA -----
) A0

UNION ALL

SELECT
A1."001-003(3)"--Tipo Registro 
||A1."004-004(1)"--DOI Tipo
||A1."005-016(12)"--DOI Número
||A1."017-017(1)"--Tipo de Abono
||A1."018-037(20)"--Número de cuenta de abono
||A1."038-077(40)"--Nombre Beneficiario
||A1."078-092(15)"--Importe a abonar
||A1."093-093(1)"--Tipo de documento
||A1."094-105(12)"--Número de documento
||A1."106-106(1)"--Abono Agrupado

||A1."107-146(40)"--Referencia-Opcional
||A1."147-147(1)"--Indicador de aviso -opcional
||A1."148-197(50)"--Medio de aviso-opcional
||A1."198-227(30)"--Persona Contacto - opcional

||A1."228-229(2)"--Indicador de proceso -banco
||A1."230-259(30)"--Descripción -banco
||A1."260-277(18)"--Filler-banco
||'|'
AS "Resultado"

FROM (
	----- DETALLE -----
SELECT
	
	'002' 							AS "001-003(3)",--Tipo Registro 
	CASE T3."U_EXX_TIPODOCU"
		WHEN '1' THEN 'L'
		WHEN '4' THEN 'E '
		WHEN '6' THEN 'R'
		WHEN '7' THEN 'P'
		ELSE '' END 				AS "004-004(1)",--DOI Tipo
	LEFT(T3."LicTradNum"||replicate(' ',12),12) AS "005-016(12)",--DOI Número
	CASE WHEN T1."U_EXP_CODBANCOPROV" = '011' THEN 'P' ELSE 'I' END AS "017-017(1)",--Tipo de Abono - P: Propio Banco I:interbancario O: Orden Pago
	CASE T1."U_EXP_MEDIODEPAGO"
	WHEN 
		'CG' THEN replicate(' ',20)
	ELSE
		CASE T1."U_EXP_CODBANCOPROV" WHEN :bankCode THEN
				/*
				REPLACE(
				LEFT(LEFT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),3)
				||RIGHT(replicate('0',8)||LEFT(RIGHT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),LENGTH(REPLACE(T1."U_EXP_NROCTAPROV",'-',''))-3),LENGTH(RIGHT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),LENGTH(REPLACE(T1."U_EXP_NROCTAPROV",'-',''))-3))-3),8)
				||RIGHT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),3)
				||replicate(' ',20),20)
				*/
			case 
				when length(replace(T1."U_EXP_NROCTAPROV",'-','')) = 20 then replace(T1."U_EXP_NROCTAPROV",'-','')
				when length(replace(T1."U_EXP_NROCTAPROV",'-','')) = 18 then 
					left(replace(T1."U_EXP_NROCTAPROV",'-',''),8) ||'00'||right(replace(T1."U_EXP_NROCTAPROV",'-',''),length(replace(T1."U_EXP_NROCTAPROV",'-',''))-8) 
			else 
				lpad('',20,'0') 
			end
			/*LEFT(RTRIM(LEFT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),8)) ||'00'|| LTRIM(SUBSTRING(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),9)) || replicate(' ',20),20)*/
		ELSE 
			LEFT(REPLACE(T1."U_EXP_NROCTAPROV",'-','')||replicate(' ',20),20) 
		END 
	END AS  "018-037(20)",--Número de cuenta de abono
	LEFT(LIMPIA_CADENA(T1."U_EXP_CARDNAME")||replicate(' ',40),40) as "038-077(40)",--Nombre Beneficiario
	RIGHT(replicate('0',15)||REPLACE(REPLACE(CAST(CAST(ROUND(T1."U_EXP_IMPORTE",2) AS DECIMAL(18,2)) AS NVARCHAR(20)),',',''),'.',''),15) AS "078-092(15)",--Importe a abonar
	CASE T1."U_EXP_MEDIODEPAGO"
	WHEN 'TB' THEN
		CASE coalesce(T4."Indicator",T5."Indicator")
		WHEN '00' THEN 'F'
		WHEN '01' THEN 'F'
		WHEN '02' THEN 'B'
		WHEN '03' THEN 'B'
		WHEN '07' THEN 'N'
		--ELSE 'D' END
		ELSE 'F' END
	WHEN 'CG' THEN 'F'
	ELSE ' ' END AS "093-093(1)",--Tipo de documetno
	rpad(coalesce(left(T1."U_EXP_NROSUNAT",12),''),12,' ') AS "094-105(12)",--Número de documento
	'N' as "106-106(1)",--Abono Agrupado - S=Abono Agrupado N=Abono Individual
	rpad(coalesce(left(T1."U_EXP_NROSUNAT",12),''),40,' ')  as "107-146(40)",--Referencia-Opcional
	replicate(' ',1)  as "147-147(1)",--Indicador de aviso -opcional
	replicate(' ',50)  as "148-197(50)",--Medio de aviso-opcional
	rpad(left(LIMPIA_CADENA(T1."U_EXP_CARDNAME"),30),30,' ')  as "198-227(30)",--Persona Contacto - opcional

	replicate(' ',2)  as "228-229(2)",--Indicador de proceso -banco
	replicate(' ',30)  as "230-259(30)",--Descripción -banco
	replicate(' ',18)  as "260-277(18)" --Filler-banco
/*	CASE T1."U_EXP_MEDIODEPAGO"
		WHEN 'TB' THEN '2'
		WHEN 'CG' THEN '0'
		ELSE ' ' END AS "002-002(1)",
	CASE T1."U_EXP_MEDIODEPAGO"
		WHEN 'CG' THEN 'C'
		ELSE
			CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
				WHEN :bankCode THEN (SELECT ifnull("UsrNumber2",'') FROM OCRB WHERE "CardCode"=T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV" AND "BankCode"=:bankCode)
				ELSE 'B' END
			END AS "003-003(1)",*/
	
	
FROM "@EXP_OPMP" T0
INNER JOIN "@EXP_PMP1" T1 ON T0."DocEntry"=T1."DocEntry"
INNER JOIN DSC1 T2 ON T1."U_EXP_CODCTABANCO"=T2."GLAccount" and T2."Branch" = T1."U_EXP_COD_SUCURSAL"
INNER JOIN OCRD T3 ON T1."U_EXP_CARDCODE"=T3."CardCode"
LEFT JOIN OPCH T4 ON T1."U_EXP_DOCENTRYDOC"=T4."DocEntry" AND T1."U_EXP_TIPODOC"=T4."ObjType"
LEFT JOIN ODPO T5 ON T1."U_EXP_DOCENTRYDOC"=T5."DocEntry" AND T1."U_EXP_TIPODOC"=T5."ObjType"
LEFT JOIN OJDT T6 ON T1."U_EXP_DOCENTRYDOC"=T6."TransId"  AND 30 = T6."ObjType"
LEFT JOIN OPDF T7 ON T1."U_EXP_DOCENTRYDOC"=T7."DocEntry" AND T1."U_EXP_TIPODOC"=140
WHERE 
T0."DocEntry"=:docEntry AND
T1."U_EXP_CODBANCO"=:bankCode
AND T1."U_EXP_CODCTABANCO"=:glaccount
AND IFNULL(T2."U_EXM_PMASIVO",'') = 'Y'
AND T1."U_EXP_MEDIODEPAGO" IN ('TB','CG') AND T1."U_EXP_SLC_RETENCION"='N'
AND ((T1."U_EXP_MEDIODEPAGO"='TB' AND IFNULL(T1."U_EXP_NROCTAPROV",'')!='') OR (T1."U_EXP_MEDIODEPAGO"='CG'))
	--AND T1."U_EXP_CARDCODE"='P20603816898'
	----- FIN -----
) A1;

END IF;

END;