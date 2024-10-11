CREATE PROCEDURE SBO_EXX_PM_BBVA(
	@docEntry int, 
	@codSucursal int,
	@glaccount nvarchar(15)
)
AS
BEGIN

-- Llenado de variables
declare @factoring nvarchar(1) = (SELECT max("U_EXC_FCTRNG") FROM DSC1 WHERE "GLAccount"= @glaccount and isnull("Branch",'0') = isnull(@codSucursal,'0') and isnull("U_EXM_PMASIVO",'') = 'Y');
declare @bankCode nvarchar(5) = '011';

IF isnull(@factoring,'N') = 'N' 
SELECT
A0."001-003 (3)" --Tipo de Registro
+A0."004-023(20)"--Cuenta de Cargo
+A0."024-026(3)"--Moneda de Cuenta de Cargo
+A0."027-041(15)"--Importe a Cargar
+A0."042-042(1)"--Tipo de Proceso A Inmediato HHora FFecha
+A0."043-050(8)"--Fecha de Proceso - Opcional si Tipo es F
+A0."051-051(1)"--Hora Proceso - Opcional si tipo es H - B1100am C300pm D700pm
+A0."052-076(25)"--REFERENCIA opcional

+A0."077-082(6)"--TOtal de Registros
+A0."083-083(1)" --Validación de pertencia S Valida, si hay error rechaza abono N Novalida

+A0."084-098(15)" --Valor de control -Banco
+A0."099-101(3)" --Indicador de proceso-Banco
+A0."102-131(30)" --Descripción --Banco
+A0."132-146(15)" --IMporte máximo por registro-opcional
+A0."147-151(5)" --Filler-Banco -Banco
+'|'
AS "Resultado"

FROM (
	----- CABECERA -----
	SELECT
	'750' AS "001-003 (3)",--TIPO REGISTRO
	case 
		when len(replace(T2."Account",'-','')) = 20 then replace(T2."Account",'-','')
		when len(replace(T2."Account",'-','')) = 18 then 
			left(replace(T2."Account",'-',''),8) +'00'+right(replace(T2."Account",'-',''),len(replace(T2."Account",'-',''))-8) 
		else replicate('0',20) end AS "004-023(20)",--CUENTA CARGO
	CASE T2."UsrNumber4" WHEN 'SOL' THEN 'PEN' WHEN 'USD' THEN 'USD' ELSE '' END AS "024-026(3)",--MONEDA
	RIGHT(replicate('0',15)+REPLACE(REPLACE(CAST(CAST(SUM(T1."U_EXP_IMPORTE") AS DECIMAL(18,2)) AS NVARCHAR(20)),',',''),'.',''),15) AS "027-041(15)", -- IMPORTE A CARGAR
	'A'  as "042-042(1)",--Tipo de Proceso A Inmediato HHora FFecha
	replicate(' ',8) AS "043-050(8)", --Fecha de Proceso - Opcional si Tipo es F
	' ' as "051-051(1)", -- --Hora Proceso - Opcional si tipo es H - B1100am C300pm D700pm
	replicate(' ',25) AS "052-076(25)",--Referencia
	right(replicate('0',6) + cast(SUM(1) as varchar),6) "077-082(6)",--TOtal de Registros

	'S' as "083-083(1)",  --Validación de pertencia S Valida, si hay error rechaza abono N Novalida

	replicate(' ',15) as "084-098(15)", --Valor de control - banco
	replicate(' ',3)  as "099-101(3)", --Indicador de proceso - banco
	replicate(' ',30) as "102-131(30)", --Descripción - banco
	replicate(' ',15) as "132-146(15)", --IMporte máximo por registro-opcional
	replicate(' ',5) as "147-151(5)", --Filler - banco

	RIGHT(replicate('0',15)+
		CAST(
		CAST(RIGHT(replicate('0',8)+LEFT(RIGHT(REPLACE(T2."Account",'-',''),len(REPLACE(T2."Account",'-',''))-3),len(RIGHT(REPLACE(T2."Account",'-',''),len(REPLACE(T2."Account",'-',''))-3))-3),8)
			+RIGHT(REPLACE(T2."Account",'-',''),3) AS BIGINT)
		+
		SUM(CAST(RIGHT(
			RIGHT(replicate('0',8)+LEFT(RIGHT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),len(REPLACE(T1."U_EXP_NROCTAPROV",'-',''))-3),len(RIGHT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),len(REPLACE(T1."U_EXP_NROCTAPROV",'-',''))-3))-3),8)
			+RIGHT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),3)
			,15) AS BIGINT))
		AS NVARCHAR(100)),15)
		AS "070-084(15)", -- checksum
	RIGHT(replicate('0',6)+COUNT(T1."DocEntry"),6) AS "085-090(6)",
	'0' AS "091-091(1)",
	replicate(' ',15) AS "092-106(15)",
	'0' AS "107-107(1)" -- Nota de cargo 0no 1si
	
	FROM "@EXP_OPMP" T0
	INNER JOIN "@EXP_PMP1" T1 ON T0."DocEntry" = T1."DocEntry"
	INNER JOIN DSC1 T2 ON T1."U_EXP_CODCTABANCO" = T2."GLAccount" and isnull(T2."Branch",'0') = T1."U_EXP_COD_SUCURSAL"
	WHERE 
	T0."DocEntry"= @docEntry
	 
	AND T1."U_EXP_CODBANCO"=@bankCode 
	AND T1."U_EXP_CODCTABANCO"	=	@glaccount
	AND T1."U_EXP_COD_SUCURSAL"	=	@codSucursal
	AND T1.U_EXP_SLC_PAGO = 'Y'
	AND isnull(T2."U_EXM_PMASIVO",'') = 'Y'
	AND T1."U_EXP_MEDIODEPAGO" IN ('TB','CG') AND T1."U_EXP_SLC_RETENCION"='N'
	--AND T1."U_EXP_CARDCODE"='P20603816898'
	AND ((T1."U_EXP_MEDIODEPAGO"='TB' AND isnull(T1."U_EXP_NROCTAPROV",'')!='') OR (T1."U_EXP_MEDIODEPAGO"='CG'))
	GROUP BY T2."UsrNumber4",T2."UsrNumber2",T2."Account"--,T0."U_EXP_FECHAPAGO"
	----- FIN CABECERA -----
) A0

UNION ALL

SELECT
A1."001-003(3)"--Tipo Registro 
+A1."004-004(1)"--DOI Tipo
+A1."005-016(12)"--DOI Número
+A1."017-017(1)"--Tipo de Abono
+A1."018-037(20)"--Número de cuenta de abono
+A1."038-077(40)"--Nombre Beneficiario
+A1."078-092(15)"--Importe a abonar
+A1."093-093(1)"--Tipo de documento
+A1."094-105(12)"--Número de documento
+A1."106-106(1)"--Abono Agrupado

+A1."107-146(40)"--Referencia-Opcional
+A1."147-147(1)"--Indicador de aviso -opcional
+A1."148-197(50)"--Medio de aviso-opcional
+A1."198-227(30)"--Persona Contacto - opcional

+A1."228-229(2)"--Indicador de proceso -banco
+A1."230-259(30)"--Descripción -banco
+A1."260-277(18)"--Filler-banco
+'|'
AS "Resultado"

FROM (
	----- DETALLE -----
	SELECT
	
'002' as "001-003(3)",--Tipo Registro 
CASE T3."U_EXX_TIPODOCU"
		WHEN '1' THEN 'L'
		WHEN '4' THEN 'E '
		WHEN '6' THEN 'R'
		WHEN '7' THEN 'P'
		ELSE '' END AS "004-004(1)",--DOI Tipo
LEFT(T3."LicTradNum"+replicate(' ',12),12) AS "005-016(12)",--DOI Número
CASE WHEN T1."U_EXP_CODBANCOPROV" = '011' THEN 'P' ELSE 'I' END AS "017-017(1)",--Tipo de Abono - P Propio Banco Iinterbancario O Orden Pago
CASE T1."U_EXP_MEDIODEPAGO"
		WHEN 'CG' THEN replicate(' ',20)
		ELSE
			CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV") WHEN @bankCode THEN
				/*
				REPLACE(
				LEFT(LEFT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),3)
				+RIGHT(replicate('0',8)+LEFT(RIGHT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),len(REPLACE(T1."U_EXP_NROCTAPROV",'-',''))-3),len(RIGHT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),len(REPLACE(T1."U_EXP_NROCTAPROV",'-',''))-3))-3),8)
				+RIGHT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),3)
				+replicate(' ',20),20)
				*/
				LEFT(RTRIM(LEFT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),8)) +'00'+ LTRIM(right(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),10)) + replicate(' ',20),20)
			ELSE LEFT(REPLACE(T1."U_EXP_NROCTAPROV",'-','')+replicate(' ',20),20) END END AS  "018-037(20)",--Número de cuenta de abono
LEFT(dbo.LIMPIA_CADENA(T1."U_EXP_CARDNAME")+replicate(' ',40),40) as "038-077(40)",--Nombre Beneficiario
RIGHT(replicate('0',15)+REPLACE(REPLACE(CAST(CAST(T1."U_EXP_IMPORTE" AS DECIMAL(18,2)) AS NVARCHAR(20)),',',''),'.',''),15) AS "078-092(15)",--Importe a abonar
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
left(coalesce(cast(T4."NumAtCard" as varchar),cast(T5."NumAtCard" as varchar),cast(T6."TransId" as varchar),cast(T7."DocEntry" as varchar)) + replicate(' ',12),12)	AS "094-105(12)",--Número de documento
'N' as "106-106(1)",--Abono Agrupado - S=Abono Agrupado N=Abono Individual
left(coalesce(cast(T4."NumAtCard" as varchar),cast(T5."NumAtCard" as varchar),cast(T6."TransId" as varchar),cast(T7."DocEntry" as varchar)) + replicate(' ',40),40)	as "107-146(40)",--Referencia-Opcional
replicate(' ',1)  as "147-147(1)",--Indicador de aviso -opcional
replicate(' ',50)  as "148-197(50)",--Medio de aviso-opcional
left(left(dbo.LIMPIA_CADENA(T1."U_EXP_CARDNAME"),30) + replicate(' ',30),30)  as "198-227(30)",--Persona Contacto - opcional

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
				WHEN @bankCode THEN (SELECT isnull("UsrNumber2",'') FROM OCRB WHERE "CardCode"=T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV" AND "BankCode"=@bankCode)
				ELSE 'B' END
			END AS "003-003(1)",*/
	
	
	FROM "@EXP_OPMP" T0
	INNER JOIN "@EXP_PMP1" T1 ON T0."DocEntry"=T1."DocEntry"
	INNER JOIN DSC1 T2 ON T1."U_EXP_CODCTABANCO"=T2."GLAccount" and isnull(T2."Branch",'0') = T1."U_EXP_COD_SUCURSAL"
	INNER JOIN OCRD T3 ON T1."U_EXP_CARDCODE"=T3."CardCode"
	LEFT JOIN OPCH T4 ON T1."U_EXP_DOCENTRYDOC"=T4."DocEntry" AND T1."U_EXP_TIPODOC"=T4."ObjType"
	LEFT JOIN ODPO T5 ON T1."U_EXP_DOCENTRYDOC"=T5."DocEntry" AND T1."U_EXP_TIPODOC"=T5."ObjType"
	LEFT JOIN OJDT T6 ON T1."U_EXP_DOCENTRYDOC"=T6."TransId"  AND 30 = T6."ObjType"
	LEFT JOIN OPDF T7 ON T1."U_EXP_DOCENTRYDOC"=T7."DocEntry" AND T1."U_EXP_TIPODOC"=140
	WHERE 
	T0."DocEntry"=@docEntry AND
	T1."U_EXP_CODBANCO"=@bankCode
	AND T1."U_EXP_CODCTABANCO"=@glaccount
	AND isnull(T2."U_EXM_PMASIVO",'') = 'Y'
	and T1.U_EXP_SLC_PAGO = 'Y'
	AND T1."U_EXP_MEDIODEPAGO" IN ('TB','CG') AND T1."U_EXP_SLC_RETENCION"='N'
	AND ((T1."U_EXP_MEDIODEPAGO"='TB' AND isnull(T1."U_EXP_NROCTAPROV",'')!='') OR (T1."U_EXP_MEDIODEPAGO"='CG'))
	--AND T1."U_EXP_CARDCODE"='P20603816898'
	----- FIN -----
) A1;

END

