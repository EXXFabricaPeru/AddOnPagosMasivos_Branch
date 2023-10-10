CREATE PROCEDURE SBO_EXX_PM_SANTANDER (
docEntry int, glaccount nvarchar(15)
)

AS
factoring nvarchar(1);
BEGIN
-- Llenado de variables
SELECT "U_EXC_FCTRNG" INTO factoring FROM DSC1 WHERE "GLAccount"=:glaccount;

IF :factoring='N' THEN

SELECT
LEFT(A."Tipo docmto.Proveedor"||replicate(' ',1),1)||
LEFT(A."Nro.docmto.Proveedor"||replicate(' ',11),11)||
LEFT(A."Tipo documento pago"||replicate(' ',2),2)||
LEFT(A."Nro.Documento pago"||replicate(' ',12),12)||
LEFT(A."Moneda"||replicate(' ',2),2)||
RIGHT(replicate('0',15)||A."Importe del Documento",15)||
LEFT(A."Fecha de pago"||replicate(' ',10),10)||
LEFT(A."Concepto"||replicate(' ',20),20)||
LEFT(A."Forma de Pago"||replicate(' ',2),2)||
LEFT(A."Cuenta de abono"||replicate(' ',10),10)||
LEFT(A."Cuenta CCE"||replicate(' ',20),20)||
LEFT(A."Nombres Beneficiado"||replicate(' ',14),14)||
LEFT(A."Apellido Paterno Beneficiado"||replicate(' ',15),15)||
LEFT(A."Apellido Materno Beneficiado"||replicate(' ',15),15)||
LEFT(A."Tipo persona Beneficiado"||replicate(' ',1),1)||
LEFT(A."Nombre Empresa Beneficiada"||replicate(' ',44),44)||
LEFT(A."Referencia Cliente"||replicate(' ',30),30)
||'|'
	AS "Resultado"

FROM (
	----- OPCH -----
	SELECT
	CASE 
		CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_TIPODOCU"
			ELSE (
				SELECT C1."U_EXX_TIPDOC"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		WHEN '1' THEN '1'
		WHEN '6' THEN '2'
		WHEN '4' THEN '3'
		ELSE '0' END AS "Tipo docmto.Proveedor",
	CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."LicTradNum"
			ELSE (
				SELECT C1."U_EXX_NUMDOC"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END AS "Nro.docmto.Proveedor",
	CASE T4."Indicator"
		WHEN '01' THEN 'FA'
		WHEN '08' THEN 'DP'
		WHEN '03' THEN 'BV'
		ELSE 'OT' END AS "Tipo documento pago",
	CASE IFNULL(T4."NumAtCard",'') WHEN '' THEN T1."U_EXP_DOCENTRYDOC" ELSE T4."NumAtCard" END AS "Nro.Documento pago",
	CASE T1."U_EXP_MONEDA" WHEN 'SOL' THEN '01' WHEN 'USD' THEN '02' ELSE '' END AS "Moneda",
	REPLACE(CAST(CAST(T1."U_EXP_IMPORTE" AS DECIMAL(18,2)) AS NVARCHAR(11)),',','') AS "Importe del Documento",
	TO_NVARCHAR(T0."U_EXP_FECHAPAGO",'DD-MM-YYYY') AS "Fecha de pago",
	'' AS "Concepto",
	CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode"=T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
		WHEN '022' THEN '01'
		ELSE '03' END AS "Forma de Pago",
	CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode"=T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
		WHEN '022' THEN REPLACE(T1."U_EXP_NROCTAPROV",'-','')
		ELSE '' END AS "Cuenta de abono",
	CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode"=T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
		WHEN '022' THEN ''
		ELSE REPLACE(T1."U_EXP_NROCTAPROV",'-','') END AS "Cuenta CCE",
	CASE 
		CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_TIPODOCU"
			ELSE (
				SELECT C1."U_EXX_TIPDOC"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		WHEN '1' THEN		
			CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_PRIMERNO"||IFNULL(' '||T3."U_EXX_SEGUNDNO",'')
			ELSE (
				SELECT C1."FirstName"||IFNULL(' '||C1."MiddleName",'')
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		ELSE '' END AS "Nombres Beneficiado",
	CASE 
		CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_TIPODOCU"
			ELSE (
				SELECT C1."U_EXX_TIPDOC"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		WHEN '1' THEN		
			CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_APELLPAT"
			ELSE (
				SELECT C1."LastName"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		ELSE '' END AS "Apellido Paterno Beneficiado",
	CASE 
		CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_TIPODOCU"
			ELSE (
				SELECT C1."U_EXX_TIPDOC"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		WHEN '1' THEN		
			CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_APELLMAT"
			ELSE '' END
		ELSE '' END AS "Apellido Materno Beneficiado",
	CASE T3."U_EXX_TIPOPERS"
		WHEN 'TPN' THEN 'N'
		WHEN 'TPJ' THEN 'J'
		ELSE '' END AS "Tipo persona Beneficiado",
		
	CASE 
		CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_TIPODOCU"
			ELSE (
				SELECT C1."U_EXX_TIPDOC"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		WHEN '6' THEN		
			CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."CardName"
			ELSE (
				SELECT C1."FirstName"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		ELSE '' END AS "Nombre Empresa Beneficiada",
	'' AS "Referencia Cliente"
	
	FROM "@EXP_OPMP" T0
	INNER JOIN "@EXP_PMP1" T1 ON T0."DocEntry"=T1."DocEntry"
	INNER JOIN DSC1 T2 ON T1."U_EXP_CODCTABANCO"=T2."GLAccount"
	INNER JOIN OCRD T3 ON T1."U_EXP_CARDCODE"=T3."CardCode"
	INNER JOIN OPCH T4 ON T4."ObjType"=T1."U_EXP_TIPODOC" AND T4."DocEntry"=T1."U_EXP_DOCENTRYDOC"
	WHERE T0."DocEntry"=:docEntry AND T1."U_EXP_MEDIODEPAGO" IN ('TB','CG')
	AND T1."U_EXP_CODBANCO"='022' AND T1."U_EXP_CODCTABANCO"=:glaccount
	AND IFNULL(T1."U_EXP_NROCTAPROV",'')!='' AND T1."U_EXP_SLC_RETENCION"='N'
	
	UNION ALL
	
	----- ODPO -----
	SELECT
	CASE 
		CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_TIPODOCU"
			ELSE (
				SELECT C1."U_EXX_TIPDOC"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		WHEN '1' THEN '1'
		WHEN '6' THEN '2'
		WHEN '4' THEN '3'
		ELSE '0' END AS "Tipo docmto.Proveedor",
	CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."LicTradNum"
			ELSE (
				SELECT C1."U_EXX_NUMDOC"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END AS "Nro.docmto.Proveedor",
	CASE T4."Indicator"
		WHEN '01' THEN 'FA'
		WHEN '08' THEN 'DP'
		WHEN '03' THEN 'BV'
		ELSE 'OT' END AS "Tipo documento pago",
	CASE IFNULL(T4."NumAtCard",'') WHEN '' THEN T1."U_EXP_DOCENTRYDOC" ELSE T4."NumAtCard" END AS "Nro.Documento pago",
	CASE T1."U_EXP_MONEDA" WHEN 'SOL' THEN '01' WHEN 'USD' THEN '02' ELSE '' END AS "Moneda",
	REPLACE(CAST(CAST(T1."U_EXP_IMPORTE" AS DECIMAL(18,2)) AS NVARCHAR(11)),',','') AS "Importe del Documento",
	TO_NVARCHAR(T0."U_EXP_FECHAPAGO",'DD-MM-YYYY') AS "Fecha de pago",
	'' AS "Concepto",
	CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode"=T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
		WHEN '022' THEN '01'
		ELSE '03' END AS "Forma de Pago",
	CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode"=T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
		WHEN '022' THEN REPLACE(T1."U_EXP_NROCTAPROV",'-','')
		ELSE '' END AS "Cuenta de abono",
	CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode"=T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
		WHEN '022' THEN ''
		ELSE REPLACE(T1."U_EXP_NROCTAPROV",'-','') END AS "Cuenta CCE",
	CASE 
		CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_TIPODOCU"
			ELSE (
				SELECT C1."U_EXX_TIPDOC"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		WHEN '1' THEN		
			CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_PRIMERNO"||IFNULL(' '||T3."U_EXX_SEGUNDNO",'')
			ELSE (
				SELECT C1."FirstName"||IFNULL(' '||C1."MiddleName",'')
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		ELSE '' END AS "Nombres Beneficiado",
	CASE 
		CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_TIPODOCU"
			ELSE (
				SELECT C1."U_EXX_TIPDOC"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		WHEN '1' THEN		
			CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_APELLPAT"
			ELSE (
				SELECT C1."LastName"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		ELSE '' END AS "Apellido Paterno Beneficiado",
	CASE 
		CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_TIPODOCU"
			ELSE (
				SELECT C1."U_EXX_TIPDOC"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		WHEN '1' THEN		
			CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_APELLMAT"
			ELSE '' END
		ELSE '' END AS "Apellido Materno Beneficiado",
	CASE T3."U_EXX_TIPOPERS"
		WHEN 'TPN' THEN 'N'
		WHEN 'TPJ' THEN 'J'
		ELSE '' END AS "Tipo persona Beneficiado",
		
	CASE 
		CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_TIPODOCU"
			ELSE (
				SELECT C1."U_EXX_TIPDOC"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		WHEN '6' THEN		
			CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."CardName"
			ELSE (
				SELECT C1."FirstName"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		ELSE '' END AS "Nombre Empresa Beneficiada",
	'' AS "Referencia Cliente"
	
	FROM "@EXP_OPMP" T0
	INNER JOIN "@EXP_PMP1" T1 ON T0."DocEntry"=T1."DocEntry"
	INNER JOIN DSC1 T2 ON T1."U_EXP_CODCTABANCO"=T2."GLAccount"
	INNER JOIN OCRD T3 ON T1."U_EXP_CARDCODE"=T3."CardCode"
	INNER JOIN ODPO T4 ON T4."ObjType"=T1."U_EXP_TIPODOC" AND T4."DocEntry"=T1."U_EXP_DOCENTRYDOC"
	WHERE T0."DocEntry"=:docEntry AND T1."U_EXP_MEDIODEPAGO" IN ('TB','CG')
	AND T1."U_EXP_CODBANCO"='022' AND T1."U_EXP_CODCTABANCO"=:glaccount
	AND IFNULL(T1."U_EXP_NROCTAPROV",'')!='' AND T1."U_EXP_SLC_RETENCION"='N'
	
	UNION ALL
	
	----- ORIN -----
	SELECT
	CASE 
		CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_TIPODOCU"
			ELSE (
				SELECT C1."U_EXX_TIPDOC"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		WHEN '1' THEN '1'
		WHEN '6' THEN '2'
		WHEN '4' THEN '3'
		ELSE '0' END AS "Tipo docmto.Proveedor",
	CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."LicTradNum"
			ELSE (
				SELECT C1."U_EXX_NUMDOC"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END AS "Nro.docmto.Proveedor",
	CASE T4."Indicator"
		WHEN '01' THEN 'FA'
		WHEN '08' THEN 'DP'
		WHEN '03' THEN 'BV'
		ELSE 'OT' END AS "Tipo documento pago",
	CASE IFNULL(T4."NumAtCard",'') WHEN '' THEN T1."U_EXP_DOCENTRYDOC" ELSE T4."NumAtCard" END AS "Nro.Documento pago",
	CASE T1."U_EXP_MONEDA" WHEN 'SOL' THEN '01' WHEN 'USD' THEN '02' ELSE '' END AS "Moneda",
	REPLACE(CAST(CAST(T1."U_EXP_IMPORTE" AS DECIMAL(18,2)) AS NVARCHAR(11)),',','') AS "Importe del Documento",
	TO_NVARCHAR(T0."U_EXP_FECHAPAGO",'DD-MM-YYYY') AS "Fecha de pago",
	'' AS "Concepto",
	CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode"=T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
		WHEN '022' THEN '01'
		ELSE '03' END AS "Forma de Pago",
	CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode"=T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
		WHEN '022' THEN REPLACE(T1."U_EXP_NROCTAPROV",'-','')
		ELSE '' END AS "Cuenta de abono",
	CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode"=T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
		WHEN '022' THEN ''
		ELSE REPLACE(T1."U_EXP_NROCTAPROV",'-','') END AS "Cuenta CCE",
	CASE 
		CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_TIPODOCU"
			ELSE (
				SELECT C1."U_EXX_TIPDOC"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		WHEN '1' THEN		
			CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_PRIMERNO"||IFNULL(' '||T3."U_EXX_SEGUNDNO",'')
			ELSE (
				SELECT C1."FirstName"||IFNULL(' '||C1."MiddleName",'')
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		ELSE '' END AS "Nombres Beneficiado",
	CASE 
		CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_TIPODOCU"
			ELSE (
				SELECT C1."U_EXX_TIPDOC"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		WHEN '1' THEN		
			CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_APELLPAT"
			ELSE (
				SELECT C1."LastName"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		ELSE '' END AS "Apellido Paterno Beneficiado",
	CASE 
		CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_TIPODOCU"
			ELSE (
				SELECT C1."U_EXX_TIPDOC"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		WHEN '1' THEN		
			CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_APELLMAT"
			ELSE '' END
		ELSE '' END AS "Apellido Materno Beneficiado",
	CASE T3."U_EXX_TIPOPERS"
		WHEN 'TPN' THEN 'N'
		WHEN 'TPJ' THEN 'J'
		ELSE '' END AS "Tipo persona Beneficiado",
		
	CASE 
		CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_TIPODOCU"
			ELSE (
				SELECT C1."U_EXX_TIPDOC"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		WHEN '6' THEN		
			CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."CardName"
			ELSE (
				SELECT C1."FirstName"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		ELSE '' END AS "Nombre Empresa Beneficiada",
	'' AS "Referencia Cliente"
	
	FROM "@EXP_OPMP" T0
	INNER JOIN "@EXP_PMP1" T1 ON T0."DocEntry"=T1."DocEntry"
	INNER JOIN DSC1 T2 ON T1."U_EXP_CODCTABANCO"=T2."GLAccount"
	INNER JOIN OCRD T3 ON T1."U_EXP_CARDCODE"=T3."CardCode"
	INNER JOIN ORIN T4 ON T4."ObjType"=T1."U_EXP_TIPODOC" AND T4."DocEntry"=T1."U_EXP_DOCENTRYDOC"
	WHERE T0."DocEntry"=:docEntry AND T1."U_EXP_MEDIODEPAGO" IN ('TB','CG')
	AND T1."U_EXP_CODBANCO"='022' AND T1."U_EXP_CODCTABANCO"=:glaccount
	AND IFNULL(T1."U_EXP_NROCTAPROV",'')!='' AND T1."U_EXP_SLC_RETENCION"='N'
	
	UNION ALL
	
	----- PAGOS EFECTUADOS PRELIMINARES / ASIENTOS -----
	SELECT
	CASE 
		CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_TIPODOCU"
			ELSE (
				SELECT C1."U_EXX_TIPDOC"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		WHEN '1' THEN '1'
		WHEN '6' THEN '2'
		WHEN '4' THEN '3'
		ELSE '0' END AS "Tipo docmto.Proveedor",
	CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."LicTradNum"
			ELSE (
				SELECT C1."U_EXX_NUMDOC"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END AS "Nro.docmto.Proveedor",
	'OT' AS "Tipo documento pago",
	T1."U_EXP_DOCENTRYDOC" AS "Nro.Documento pago",
	CASE T1."U_EXP_MONEDA" WHEN 'SOL' THEN '01' WHEN 'USD' THEN '02' ELSE '' END AS "Moneda",
	REPLACE(CAST(CAST(T1."U_EXP_IMPORTE" AS DECIMAL(18,2)) AS NVARCHAR(11)),',','') AS "Importe del Documento",
	TO_NVARCHAR(T0."U_EXP_FECHAPAGO",'DD-MM-YYYY') AS "Fecha de pago",
	'' AS "Concepto",
	CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode"=T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
		WHEN '022' THEN '01'
		ELSE '03' END AS "Forma de Pago",
	CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode"=T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
		WHEN '022' THEN REPLACE(T1."U_EXP_NROCTAPROV",'-','')
		ELSE '' END AS "Cuenta de abono",
	CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode"=T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
		WHEN '022' THEN ''
		ELSE REPLACE(T1."U_EXP_NROCTAPROV",'-','') END AS "Cuenta CCE",
	CASE 
		CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_TIPODOCU"
			ELSE (
				SELECT C1."U_EXX_TIPDOC"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		WHEN '1' THEN		
			CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_PRIMERNO"||IFNULL(' '||T3."U_EXX_SEGUNDNO",'')
			ELSE (
				SELECT C1."FirstName"||IFNULL(' '||C1."MiddleName",'')
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		ELSE '' END AS "Nombres Beneficiado",
	CASE 
		CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_TIPODOCU"
			ELSE (
				SELECT C1."U_EXX_TIPDOC"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		WHEN '1' THEN		
			CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_APELLPAT"
			ELSE (
				SELECT C1."LastName"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		ELSE '' END AS "Apellido Paterno Beneficiado",
	CASE 
		CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_TIPODOCU"
			ELSE (
				SELECT C1."U_EXX_TIPDOC"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		WHEN '1' THEN		
			CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_APELLMAT"
			ELSE '' END
		ELSE '' END AS "Apellido Materno Beneficiado",
	CASE T3."U_EXX_TIPOPERS"
		WHEN 'TPN' THEN 'N'
		WHEN 'TPJ' THEN 'J'
		ELSE '' END AS "Tipo persona Beneficiado",
		
	CASE 
		CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."U_EXX_TIPODOCU"
			ELSE (
				SELECT C1."U_EXX_TIPDOC"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		WHEN '6' THEN		
			CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."CardName"
			ELSE (
				SELECT C1."FirstName"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name")
			END
		ELSE '' END AS "Nombre Empresa Beneficiada",
	'' AS "Referencia Cliente"
	
	FROM "@EXP_OPMP" T0
	INNER JOIN "@EXP_PMP1" T1 ON T0."DocEntry"=T1."DocEntry"
	INNER JOIN DSC1 T2 ON T1."U_EXP_CODCTABANCO"=T2."GLAccount"
	INNER JOIN OCRD T3 ON T1."U_EXP_CARDCODE"=T3."CardCode"
	WHERE T0."DocEntry"=:docEntry AND T1."U_EXP_MEDIODEPAGO" IN ('TB','CG')
	AND T1."U_EXP_CODBANCO"='022' AND T1."U_EXP_CODCTABANCO"=:glaccount
	AND IFNULL(T1."U_EXP_NROCTAPROV",'')!='' AND T1."U_EXP_SLC_RETENCION"='N'
) A;

END IF;

END;