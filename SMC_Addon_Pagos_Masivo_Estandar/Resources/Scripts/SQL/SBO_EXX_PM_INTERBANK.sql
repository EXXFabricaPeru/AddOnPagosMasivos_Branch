CREATE PROCEDURE SBO_EXX_PM_INTERBANK (
	@docEntry int, 
	@glaccount nvarchar(15)
)

AS
DECLARE @factoring nvarchar(1);
BEGIN
-- Llenado de variables
SET @factoring = (SELECT isnull("U_EXC_FCTRNG",'') FROM DSC1 WHERE "GLAccount"=@glaccount);

IF @factoring='N'

SELECT
A0."1-2 (2)"
+A0."3-22 (20)"
+A0."23-23 (1)"
+A0."24-43 (20)"
+A0."44-51 (8)"
+A0."52-53 (2)"
+A0."54-68 (15)"
+A0."69-69 (1)"
+A0."70-71 (2)"
+A0."72-74 (3)"
+A0."75-76 (2)"
+A0."77-79 (3)"
+A0."80-99 (20)"
+A0."100-100 (1)"
+A0."101-102 (2)"
+A0."103-117 (15)"
+A0."118-177 (60)"
+A0."178-179 (2)"
+A0."180-194 (15)"
+A0."195-200 (6)"
+A0."201-240 (40)"
+A0."241-380 (140)"
+'|'
AS "Resultado"

FROM (

	----- OPCH : FACTURAS DE PROVEEDORES Y '00' -----
	SELECT
	'02' AS "1-2 (2)",
	T2."LicTradNum"+replicate(' ',20-LEN(T2."LicTradNum")) AS "3-22 (20)",
	'F' AS "23-23 (1)",
	--T1."U_EXP_DOCENTRYDOC"+replicate(' ',20-LENGTH(T1."U_EXP_DOCENTRYDOC")) AS "24-43 (20)",
	--T1."U_EXP_DOCENTRYDOC"+replicate(' ',20-LENGTH(T1."U_EXP_DOCENTRYDOC")) AS "24-43 (20)",
	isnull(REPLACE(T3."NumAtCard", '-', '')+replicate(' ',20-LEN(REPLACE(T3."NumAtCard", '-', ''))),'') AS "24-43 (20)",
	--TO_NVARCHAR(T3."DocDueDate",'YYYYMMDD') AS "44-51 (8)",
	CONVERT(nvarchar,T3."DocDueDate", 112)  AS "44-51 (8)",
	CASE T1."U_EXP_MONEDA" WHEN 'SOL' THEN '01' WHEN 'USD' THEN '10' ELSE '00' END AS "52-53 (2)",
	RIGHT(replicate('0',15)+REPLACE(REPLACE(CAST(CAST(T1."U_EXP_IMPORTE" AS DECIMAL(18,2)) AS NVARCHAR(15)),',',''),'.',''),15)
		AS "54-68 (15)",
	' ' AS "69-69 (1)",
	CASE T1."U_EXP_MEDIODEPAGO"
		WHEN 'CG' THEN '11'
		WHEN 'TB' THEN
			CASE T1."U_EXP_CODBANCOPROV"
				WHEN '003' THEN '09'
				ELSE '99' END
		ELSE replicate(' ',2) END AS "70-71 (2)",
	
	CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN
		CASE T1."U_EXP_CODBANCOPROV"
			WHEN '003' THEN (SELECT COALESCE("UsrNumber2",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
			ELSE replicate(' ',3) END
		ELSE replicate(' ',3) END AS "72-74 (3)",
	
	CASE T1."U_EXP_MEDIODEPAGO"
		WHEN 'TB' THEN
			CASE T1."U_EXP_CODBANCOPROV"
				WHEN '003' THEN
					CASE T1."U_EXP_MONEDA"
						WHEN 'SOL' THEN '01'
						WHEN 'USD' THEN '10'
						ELSE '00' END
				ELSE replicate(' ',2) END
		ELSE replicate(' ',2) END AS "75-76 (2)",
	
	CASE T1."U_EXP_MEDIODEPAGO"
		WHEN 'TB' THEN
			CASE T1."U_EXP_CODBANCOPROV"
				WHEN '003' THEN LEFT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),3)
				ELSE replicate(' ',3) END
		ELSE replicate(' ',3) END AS "77-79 (3)",
	
	CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN
		CASE T1."U_EXP_CODBANCOPROV" --T1."CAMPO BANCO DE SN"
			WHEN '003' THEN LEFT(LEFT(REPLACE(SUBSTRING(T1."U_EXP_NROCTAPROV",1,4),'-',''),10)+replicate(' ',20),20)
			ELSE LEFT(REPLACE(T1."U_EXP_NROCTAPROV",'-','')+replicate(' ',20),20)
			END ELSE replicate(' ',20) END AS "80-99 (20)",
			
	CASE T2."U_EXX_TIPOPERS"
		WHEN 'TPN' THEN 'P'
		WHEN 'TPJ' THEN 'C'
		ELSE '' END AS "100-100 (1)",
	CASE T2."U_EXX_TIPODOCU"
		WHEN '1' THEN '01'
		WHEN '6' THEN '02'
		WHEN '4' THEN '03'
		WHEN '7' THEN '05'
		ELSE '' END AS "101-102 (2)",
	CASE T3."U_EXC_PAGBEN"
		WHEN 'Y' THEN (SELECT COALESCE("U_EXX_NUMDOC"+replicate(' ',15-LEN("U_EXX_NUMDOC")),replicate(' ',15)) FROM OCPR WHERE "CardCode"=T2."CardCode" AND "U_EXC_BENEFI"='Y')
		ELSE T2."LicTradNum"+replicate(' ',15-LEN(T2."LicTradNum"))
		END AS "103-117 (15)",
	CASE T3."U_EXC_PAGBEN"
		WHEN 'Y' THEN (SELECT LEFT(COALESCE("FirstName"+replicate(' ',60-LEN("Name")),replicate(' ',60)),60) FROM OCPR WHERE "CardCode"=T2."CardCode" AND "U_EXC_BENEFI"='Y')
		ELSE LEFT(T2."CardName"+replicate(' ',60-LEN(T2."CardName")),60)
		END AS "118-177 (60)",
	replicate(' ',2) AS "178-179 (2)", -- solo pago CTS
	replicate('0',15) AS "180-194 (15)", -- solo pago CTS
	replicate(' ',6) AS "195-200 (6)",
	replicate(' ',40) AS "201-240 (40)",
	LEFT(COALESCE(COALESCE(T2."E_Mail",''),'')+replicate(' ',140),140) AS "241-380 (140)"
	
	FROM "@EXP_OPMP" T0
	INNER JOIN "@EXP_PMP1" T1 ON T0."DocEntry"=T1."DocEntry"
	INNER JOIN OCRD T2 ON T1."U_EXP_CARDCODE"=T2."CardCode"
	INNER JOIN OPCH T3 ON T1."U_EXP_DOCENTRYDOC"=T3."DocEntry"
	WHERE T0."DocEntry"=@docEntry
	AND T1."U_EXP_CODBANCO"='003' AND T1."U_EXP_CODCTABANCO"=@glaccount
	AND T1."U_EXP_TIPODOC"='18' AND T3."Indicator" IN ('00','01','02','14','50','99','05','91','SA')
	AND COALESCE(T1."U_EXP_NROCTAPROV",'')!='' AND T1."U_EXP_MEDIODEPAGO" IN ('TB','CG')
	AND T1."U_EXP_SLC_RETENCION"='N'
	----- FIN -----

	UNION ALL
	
	----- OPCH : NOTAS DE DEBITO -----
	SELECT
	'02' AS "1-2 (2)",
	T2."LicTradNum"+replicate(' ',20-LEN(T2."LicTradNum")) AS "3-22 (20)",
	'C' AS "23-23 (1)",
	REPLACE(T3."NumAtCard", '-', '')+replicate(' ',20-LEN(REPLACE(T3."NumAtCard", '-', ''))) AS "24-43 (20)",
	--T1."U_EXP_DOCENTRYDOC"+replicate(' ',20-LENGTH(T1."U_EXP_DOCENTRYDOC")) AS "24-43 (20)",
	--TO_NVARCHAR(T3."DocDueDate",'YYYYMMDD') AS "44-51 (8)",
	CONVERT(nvarchar,T3."DocDueDate", 112)   AS "44-51 (8)",
	CASE T1."U_EXP_MONEDA" WHEN 'SOL' THEN '01' WHEN 'USD' THEN '10' ELSE '00' END AS "52-53 (2)",
	RIGHT(replicate('0',15)+REPLACE(REPLACE(CAST(CAST(T1."U_EXP_IMPORTE" AS DECIMAL(18,2)) AS NVARCHAR(15)),',',''),'.',''),15)
		AS "54-68 (15)",
	' ' AS "69-69 (1)",
	CASE T1."U_EXP_MEDIODEPAGO"
		WHEN 'CG' THEN '11'
		WHEN 'TB' THEN
			CASE T1."U_EXP_CODBANCOPROV"
				WHEN '003' THEN '09'
				ELSE '99' END
		ELSE replicate(' ',2) END AS "70-71 (2)",
	
	CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN
		CASE T1."U_EXP_CODBANCOPROV"
			WHEN '003' THEN (SELECT COALESCE("UsrNumber2",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
			ELSE replicate(' ',3) END
		ELSE replicate(' ',3) END AS "72-74 (3)",
	
	CASE T1."U_EXP_MEDIODEPAGO"
		WHEN 'TB' THEN
			CASE T1."U_EXP_CODBANCOPROV"
				WHEN '003' THEN
					CASE T1."U_EXP_MONEDA"
						WHEN 'SOL' THEN '01'
						WHEN 'USD' THEN '10'
						ELSE '00' END
				ELSE replicate(' ',2) END
		ELSE replicate(' ',2) END AS "75-76 (2)",
	
	CASE T1."U_EXP_MEDIODEPAGO"
		WHEN 'TB' THEN
			CASE T1."U_EXP_CODBANCOPROV"
				WHEN '003' THEN LEFT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),3)
				ELSE replicate(' ',3) END
		ELSE replicate(' ',3) END AS "77-79 (3)",
	
	CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN
		CASE T1."U_EXP_CODBANCOPROV" --T1."CAMPO BANCO DE SN"
			WHEN '003' THEN LEFT(LEFT(REPLACE(SUBSTRING(T1."U_EXP_NROCTAPROV",1,4),'-',''),10)+replicate(' ',20),20)
			ELSE LEFT(REPLACE(T1."U_EXP_NROCTAPROV",'-','')+replicate(' ',20),20)
			END ELSE replicate(' ',20) END AS "80-99 (20)",
	CASE T2."U_EXX_TIPOPERS"
		WHEN 'TPN' THEN 'P'
		WHEN 'TPJ' THEN 'C'
	ELSE '' END AS "100-100 (1)",
	CASE T2."U_EXX_TIPODOCU"
		WHEN '1' THEN '01'
		WHEN '6' THEN '02'
		WHEN '4' THEN '03'
		WHEN '7' THEN '05'
	ELSE '' END AS "101-102 (2)",
	CASE T3."U_EXC_PAGBEN"
		WHEN 'Y' THEN (SELECT COALESCE("U_EXX_NUMDOC"+replicate(' ',15-LEN("U_EXX_NUMDOC")),replicate(' ',15)) FROM OCPR WHERE "CardCode"=T2."CardCode" AND "U_EXC_BENEFI"='Y')
		ELSE T2."LicTradNum"+replicate(' ',15-LEN(T2."LicTradNum"))
		END AS "103-117 (15)",
	CASE T3."U_EXC_PAGBEN"
		WHEN 'Y' THEN (SELECT LEFT(COALESCE("FirstName"+replicate(' ',60-LEN("Name")),replicate(' ',60)),60) FROM OCPR WHERE "CardCode"=T2."CardCode" AND "U_EXC_BENEFI"='Y')
		ELSE T2."CardName"+replicate(' ',60-LEN(T2."CardName"))
		END AS "118-177 (60)",
	replicate(' ',2) AS "178-179 (2)", -- solo pago CTS
	replicate('0',15) AS "180-194 (15)", -- solo pago CTS
	replicate(' ',6) AS "195-200 (6)",
	replicate(' ',40) AS "201-240 (40)",
	LEFT(COALESCE(T2."E_Mail",'')+replicate(' ',140),140) AS "241-380 (140)"
	
	FROM "@EXP_OPMP" T0
	INNER JOIN "@EXP_PMP1" T1 ON T0."DocEntry"=T1."DocEntry"
	INNER JOIN OCRD T2 ON T1."U_EXP_CARDCODE"=T2."CardCode"
	INNER JOIN OPCH T3 ON T1."U_EXP_DOCENTRYDOC"=T3."DocEntry"
	WHERE T0."DocEntry"=@docEntry
	AND T1."U_EXP_CODBANCO"='003' AND T1."U_EXP_CODCTABANCO"=@glaccount
	AND T1."U_EXP_TIPODOC"='18' AND T3."Indicator"='08'
	AND COALESCE(T1."U_EXP_NROCTAPROV",'')!='' AND T1."U_EXP_MEDIODEPAGO" IN ('TB','CG')
	AND T1."U_EXP_SLC_RETENCION"='N'
	----- FIN -----

	UNION ALL

	----- ODPO : FACTURAS DE ANTICIPO -----
	SELECT
	'02' AS "1-2 (2)",
	T2."LicTradNum"+replicate(' ',20-LEN(T2."LicTradNum")) AS "3-22 (20)",
	'F' AS "23-23 (1)",
	--T1."U_EXP_DOCENTRYDOC"+replicate(' ',20-LENGTH(T1."U_EXP_DOCENTRYDOC")) AS "24-43 (20)",
	REPLACE(T3."NumAtCard", '-', '')+replicate(' ',20-LEN(REPLACE(T3."NumAtCard", '-', ''))) AS "24-43 (20)",
	--TO_NVARCHAR(T3."DocDueDate",'YYYYMMDD') AS "44-51 (8)",
	CONVERT(nvarchar,T3."DocDueDate", 112) AS "44-51 (8)",
	CASE T1."U_EXP_MONEDA" WHEN 'SOL' THEN '01' WHEN 'USD' THEN '10' ELSE '00' END AS "52-53 (2)",
	RIGHT(replicate('0',15)+REPLACE(REPLACE(CAST(CAST(T1."U_EXP_IMPORTE" AS DECIMAL(18,2)) AS NVARCHAR(15)),',',''),'.',''),15)
		AS "54-68 (15)",
	' ' AS "69-69 (1)",
	CASE T1."U_EXP_MEDIODEPAGO"
		WHEN 'CG' THEN '11'
		WHEN 'TB' THEN
			CASE T1."U_EXP_CODBANCOPROV"
				WHEN '003' THEN '09'
				ELSE '99' END
		ELSE replicate(' ',2) END AS "70-71 (2)",
	
	CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN
		CASE T1."U_EXP_CODBANCOPROV"
			WHEN '003' THEN (SELECT COALESCE("UsrNumber2",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
			ELSE replicate(' ',3) END
		ELSE replicate(' ',3) END AS "72-74 (3)",
	
	CASE T1."U_EXP_MEDIODEPAGO"
		WHEN 'TB' THEN
			CASE T1."U_EXP_CODBANCOPROV"
				WHEN '003' THEN
					CASE T1."U_EXP_MONEDA"
						WHEN 'SOL' THEN '01'
						WHEN 'USD' THEN '10'
						ELSE '00' END
				ELSE replicate(' ',2) END
		ELSE replicate(' ',2) END AS "75-76 (2)",
	
	CASE T1."U_EXP_MEDIODEPAGO"
		WHEN 'TB' THEN
			CASE T1."U_EXP_CODBANCOPROV"
				WHEN '003' THEN LEFT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),3)
				ELSE replicate(' ',3) END
		ELSE replicate(' ',3) END AS "77-79 (3)",
	
	CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN
		CASE T1."U_EXP_CODBANCOPROV" --T1."CAMPO BANCO DE SN"
			WHEN '003' THEN LEFT(LEFT(REPLACE(SUBSTRING(T1."U_EXP_NROCTAPROV",1,4),'-',''),10)+replicate(' ',20),20)
			ELSE LEFT(REPLACE(T1."U_EXP_NROCTAPROV",'-','')+replicate(' ',20),20)
			END ELSE replicate(' ',20) END AS "80-99 (20)",
	CASE T2."U_EXX_TIPOPERS"
		WHEN 'TPN' THEN 'P'
		WHEN 'TPJ' THEN 'C'
	ELSE '' END AS "100-100 (1)",
	CASE T2."U_EXX_TIPODOCU"
		WHEN '1' THEN '01'
		WHEN '6' THEN '02'
		WHEN '4' THEN '03'
		WHEN '7' THEN '05'
	ELSE '' END AS "101-102 (2)",
	CASE T3."U_EXC_PAGBEN"
		WHEN 'Y' THEN (SELECT COALESCE("U_EXX_NUMDOC"+replicate(' ',15-LEN("U_EXX_NUMDOC")),replicate(' ',15)) FROM OCPR WHERE "CardCode"=T2."CardCode" AND "U_EXC_BENEFI"='Y')
		ELSE T2."LicTradNum"+replicate(' ',15-LEN(T2."LicTradNum"))
		END AS "103-117 (15)",
	CASE T3."U_EXC_PAGBEN"
		WHEN 'Y' THEN (SELECT LEFT(COALESCE("FirstName"+replicate(' ',60-LEN("Name")),replicate(' ',60)),60) FROM OCPR WHERE "CardCode"=T2."CardCode" AND "U_EXC_BENEFI"='Y')
		ELSE T2."CardName"+replicate(' ',60-LEN(T2."CardName"))
		END AS "118-177 (60)",
	replicate(' ',2) AS "178-179 (2)", -- solo pago CTS
	replicate('0',15) AS "180-194 (15)", -- solo pago CTS
	replicate(' ',6) AS "195-200 (6)",
	replicate(' ',40) AS "201-240 (40)",
	LEFT(COALESCE(T2."E_Mail",'')+replicate(' ',140),140) AS "241-380 (140)"
	
	FROM "@EXP_OPMP" T0
	INNER JOIN "@EXP_PMP1" T1 ON T0."DocEntry"=T1."DocEntry"
	INNER JOIN OCRD T2 ON T1."U_EXP_CARDCODE"=T2."CardCode"
	INNER JOIN ODPO T3 ON T1."U_EXP_DOCENTRYDOC"=T3."DocEntry"
	WHERE T0."DocEntry"=@docEntry
	AND T1."U_EXP_CODBANCO"='003' AND T1."U_EXP_CODCTABANCO"=@glaccount
	AND T1."U_EXP_TIPODOC"='204'
	AND COALESCE(T1."U_EXP_NROCTAPROV",'')!='' AND T1."U_EXP_MEDIODEPAGO" IN ('TB','CG')
	AND T1."U_EXP_SLC_RETENCION"='N'
	----- FIN -----

	UNION ALL
	
	----- ORIN : NOTAS DE CREDITO DE CLIENTES -----
	SELECT
	'02' AS "1-2 (2)",
	T2."LicTradNum"+replicate(' ',20-LEN(T2."LicTradNum")) AS "3-22 (20)",
	'D' AS "23-23 (1)",
	--T1."U_EXP_DOCENTRYDOC"+replicate(' ',20-LENGTH(T1."U_EXP_DOCENTRYDOC")) AS "24-43 (20)",
	REPLACE(COALESCE(T3."FolioPref",'NC01')+COALESCE(T3."FolioNum",T3."DocNum"), '-', '')+replicate(' ',20-LEN(REPLACE(COALESCE(T3."FolioPref",'NC01')+COALESCE(T3."FolioNum",T3."DocNum"), '-', ''))) AS "24-43 (20)",
	--TO_NVARCHAR(T3."DocDueDate",'YYYYMMDD') AS "44-51 (8)",
	CONVERT(nvarchar,T3."DocDueDate", 112)   AS "44-51 (8)",
	CASE T1."U_EXP_MONEDA" WHEN 'SOL' THEN '01' WHEN 'USD' THEN '10' ELSE '00' END AS "52-53 (2)",
	RIGHT(replicate('0',15)+REPLACE(REPLACE(CAST(CAST(T1."U_EXP_IMPORTE" AS DECIMAL(18,2)) AS NVARCHAR(15)),',',''),'.',''),15)
		AS "54-68 (15)",
	' ' AS "69-69 (1)",
	CASE T1."U_EXP_MEDIODEPAGO"
		WHEN 'CG' THEN '11'
		WHEN 'TB' THEN
			CASE T1."U_EXP_CODBANCOPROV"
				WHEN '003' THEN '09'
				ELSE '99' END
		ELSE replicate(' ',2) END AS "70-71 (2)",
	
	CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN
		CASE T1."U_EXP_CODBANCOPROV"
			WHEN '003' THEN (SELECT COALESCE("UsrNumber2",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
			ELSE replicate(' ',3) END
		ELSE replicate(' ',3) END AS "72-74 (3)",
	
	CASE T1."U_EXP_MEDIODEPAGO"
		WHEN 'TB' THEN
			CASE T1."U_EXP_CODBANCOPROV"
				WHEN '003' THEN
					CASE T1."U_EXP_MONEDA"
						WHEN 'SOL' THEN '01'
						WHEN 'USD' THEN '10'
						ELSE '00' END
				ELSE replicate(' ',2) END
		ELSE replicate(' ',2) END AS "75-76 (2)",
	
	CASE T1."U_EXP_MEDIODEPAGO"
		WHEN 'TB' THEN
			CASE T1."U_EXP_CODBANCOPROV"
				WHEN '003' THEN LEFT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),3)
				ELSE replicate(' ',3) END
		ELSE replicate(' ',3) END AS "77-79 (3)",
	
	CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN
		CASE T1."U_EXP_CODBANCOPROV" --T1."CAMPO BANCO DE SN"
			WHEN '003' THEN LEFT(LEFT(REPLACE(SUBSTRING(T1."U_EXP_NROCTAPROV",1,4),'-',''),10)+replicate(' ',20),20)
			ELSE LEFT(REPLACE(T1."U_EXP_NROCTAPROV",'-','')+replicate(' ',20),20)
			END ELSE replicate(' ',20) END AS "80-99 (20)",
	CASE T2."U_EXX_TIPOPERS"
		WHEN 'TPN' THEN 'P'
		WHEN 'TPJ' THEN 'C'
	ELSE '' END AS "100-100 (1)",
	CASE T2."U_EXX_TIPODOCU"
		WHEN '1' THEN '01'
		WHEN '6' THEN '02'
		WHEN '4' THEN '03'
		WHEN '7' THEN '05'
	ELSE '' END AS "101-102 (2)",
	CASE T3."U_EXC_PAGBEN"
		WHEN 'Y' THEN (SELECT COALESCE("U_EXX_NUMDOC"+replicate(' ',15-LEN("U_EXX_NUMDOC")),replicate(' ',15)) FROM OCPR WHERE "CardCode"=T2."CardCode" AND "U_EXC_BENEFI"='Y')
		ELSE T2."LicTradNum"+replicate(' ',15-LEN(T2."LicTradNum"))
		END AS "103-117 (15)",
	CASE T3."U_EXC_PAGBEN"
		WHEN 'Y' THEN (SELECT LEFT(COALESCE("FirstName"+replicate(' ',60-LEN("Name")),replicate(' ',60)),60) FROM OCPR WHERE "CardCode"=T2."CardCode" AND "U_EXC_BENEFI"='Y')
		ELSE T2."CardName"+replicate(' ',60-LEN(T2."CardName"))
		END AS "118-177 (60)",
	replicate(' ',2) AS "178-179 (2)", -- solo pago CTS
	replicate('0',15) AS "180-194 (15)", -- solo pago CTS
	replicate(' ',6) AS "195-200 (6)",
	replicate(' ',40) AS "201-240 (40)",
	LEFT(COALESCE(T2."E_Mail",'')+replicate(' ',140),140) AS "241-380 (140)"
	
	FROM "@EXP_OPMP" T0
	INNER JOIN "@EXP_PMP1" T1 ON T0."DocEntry"=T1."DocEntry"
	INNER JOIN OCRD T2 ON T1."U_EXP_CARDCODE"=T2."CardCode"
	INNER JOIN ORIN T3 ON T1."U_EXP_DOCENTRYDOC"=T3."DocEntry"
	WHERE T0."DocEntry"=@docEntry
	AND T1."U_EXP_CODBANCO"='003' AND T1."U_EXP_CODCTABANCO"=@glaccount
	AND T1."U_EXP_TIPODOC"='14'
	AND COALESCE(T1."U_EXP_NROCTAPROV",'')!='' AND T1."U_EXP_MEDIODEPAGO" IN ('TB','CG')
	AND T1."U_EXP_SLC_RETENCION"='N'
	----- FIN -----

	UNION ALL

	----- PAGOS EFECTUADOS PRELIMINARES / ASIENTOS -----
	SELECT
	'02' AS "1-2 (2)",
	T2."LicTradNum"+replicate(' ',20-LEN(T2."LicTradNum")) AS "3-22 (20)",
	'F' AS "23-23 (1)",
	T1."U_EXP_DOCENTRYDOC"+replicate(' ',20-LEN(T1."U_EXP_DOCENTRYDOC")) AS "24-43 (20)",
	--T3."DocNum"+replicate(' ',20-LENGTH(T3."DocNum")) AS "24-43 (20)",
	--TO_NVARCHAR(T0."U_EXP_FECHA",'YYYYMMDD') AS "44-51 (8)",
	CONVERT(nvarchar,T0."U_EXP_FECHA", 112) AS "44-51 (8)",
	CASE T1."U_EXP_MONEDA" WHEN 'SOL' THEN '01' WHEN 'USD' THEN '10' ELSE '00' END AS "52-53 (2)",
	RIGHT(replicate('0',15)+REPLACE(REPLACE(CAST(CAST(T1."U_EXP_IMPORTE" AS DECIMAL(18,2)) AS NVARCHAR(15)),',',''),'.',''),15)
		AS "54-68 (15)",
	' ' AS "69-69 (1)",
	CASE T1."U_EXP_MEDIODEPAGO"
		WHEN 'CG' THEN '11'
		WHEN 'TB' THEN
			CASE T1."U_EXP_CODBANCOPROV"
				WHEN '003' THEN '09'
				ELSE '99' END
		ELSE replicate(' ',2) END AS "70-71 (2)",
	
	CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN
		CASE T1."U_EXP_CODBANCOPROV"
			WHEN '003' THEN (SELECT COALESCE("UsrNumber2",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
			ELSE replicate(' ',3) END
		ELSE replicate(' ',3) END AS "72-74 (3)",
	
	CASE T1."U_EXP_MEDIODEPAGO"
		WHEN 'TB' THEN
			CASE T1."U_EXP_CODBANCOPROV"
				WHEN '003' THEN
					CASE T1."U_EXP_MONEDA"
						WHEN 'SOL' THEN '01'
						WHEN 'USD' THEN '10'
						ELSE '00' END
				ELSE replicate(' ',2) END
		ELSE replicate(' ',2) END AS "75-76 (2)",
	
	CASE T1."U_EXP_MEDIODEPAGO"
		WHEN 'TB' THEN
			CASE T1."U_EXP_CODBANCOPROV"
				WHEN '003' THEN LEFT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),3)
				ELSE replicate(' ',3) END
		ELSE replicate(' ',3) END AS "77-79 (3)",
	
	CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN
		CASE T1."U_EXP_CODBANCOPROV" --T1."CAMPO BANCO DE SN"
			WHEN '003' THEN LEFT(LEFT(REPLACE(SUBSTRING(T1."U_EXP_NROCTAPROV",1,4),'-',''),10)+replicate(' ',20),20)
			ELSE LEFT(REPLACE(T1."U_EXP_NROCTAPROV",'-','')+replicate(' ',20),20)
			END ELSE replicate(' ',20) END AS "80-99 (20)",
	CASE T2."U_EXX_TIPOPERS"
		WHEN 'TPN' THEN 'P'
		WHEN 'TPJ' THEN 'C'
	ELSE '' END AS "100-100 (1)",
	CASE T2."U_EXX_TIPODOCU"
		WHEN '1' THEN '01'
		WHEN '6' THEN '02'
		WHEN '4' THEN '03'
		WHEN '7' THEN '05'
	ELSE '' END AS "101-102 (2)",
	T2."LicTradNum"+replicate(' ',15-LEN(T2."LicTradNum")) AS "103-117 (15)",
	T2."CardName"+replicate(' ',60-LEN(T2."CardName")) AS "118-177 (60)",
	replicate(' ',2) AS "178-179 (2)", -- solo pago CTS
	replicate('0',15) AS "180-194 (15)", -- solo pago CTS
	replicate(' ',6) AS "195-200 (6)",
	replicate(' ',40) AS "201-240 (40)",
	LEFT(COALESCE(T2."E_Mail",'')+replicate(' ',140),140) AS "241-380 (140)"
	
	FROM "@EXP_OPMP" T0
	INNER JOIN "@EXP_PMP1" T1 ON T0."DocEntry"=T1."DocEntry"
	INNER JOIN OCRD T2 ON T1."U_EXP_CARDCODE"=T2."CardCode"
	WHERE T0."DocEntry"=@docEntry
	AND T1."U_EXP_CODBANCO"='003' AND T1."U_EXP_CODCTABANCO"=@glaccount
	AND T1."U_EXP_TIPODOC" IN ('30','46','140')
	AND COALESCE(T1."U_EXP_NROCTAPROV",'')!='' AND T1."U_EXP_MEDIODEPAGO" IN ('TB','CG')
	AND T1."U_EXP_SLC_RETENCION"='N'
	----- FIN -----

) A0 WHERE
A0."1-2 (2)"
+A0."3-22 (20)"
+A0."23-23 (1)"
+A0."24-43 (20)"
+A0."44-51 (8)"
+A0."52-53 (2)"
+A0."54-68 (15)"
+A0."69-69 (1)"
+A0."70-71 (2)"
+A0."72-74 (3)"
+A0."75-76 (2)"
+A0."77-79 (3)"
+A0."80-99 (20)"
+A0."100-100 (1)"
+A0."101-102 (2)"
+A0."103-117 (15)"
+A0."118-177 (60)"
+A0."178-179 (2)"
+A0."180-194 (15)"
+A0."195-200 (6)"
+A0."201-240 (40)"
+A0."241-380 (140)"
IS NOT NULL;


END;