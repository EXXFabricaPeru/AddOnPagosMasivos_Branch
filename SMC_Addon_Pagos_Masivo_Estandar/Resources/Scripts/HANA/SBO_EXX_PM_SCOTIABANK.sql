CREATE PROCEDURE SBO_EXX_PM_SCOTIABANK(
docEntry int,codSucursal int, glaccount nvarchar(15), formatoScotia varchar(2)
)

AS
RUC nvarchar(11);
factoring nvarchar(1);
BEGIN

-- Llenado de variables
SELECT "TaxIdNum" INTO RUC FROM OADM;
SELECT "U_EXC_FCTRNG" INTO factoring FROM DSC1 WHERE "GLAccount"=:glaccount 
AND "Branch" = :codSucursal AND ifnull("U_EXM_PMASIVO",'') = 'Y';




IF ifnull(:factoring,'N') = 'N' THEN
-- Estructura Pago Proveedores

	
RSLT =	SELECT
		*
		FROM (
		----- OPCH - PROVEEDORES -----
		SELECT
		CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN	
			CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
				WHEN '' THEN T3."LicTradNum"
				ELSE (
					SELECT C1."U_EXX_NUMDOC"
					FROM OCRB C0
					INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name" AND C1."U_EXC_BENEFI"='Y')
				END
			ELSE
				CASE T3."QryGroup26"
					WHEN 'Y' THEN :RUC
					ELSE CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
							WHEN '' THEN T3."LicTradNum"
							ELSE (
								SELECT C1."U_EXX_NUMDOC"
								FROM OCRB C0
								INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name" AND C1."U_EXC_BENEFI"='Y')
							END
					END
			END AS "RUC",
		CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."CardName"
			ELSE (
				SELECT C1."FirstName"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name" AND C1."U_EXC_BENEFI"='Y')
			END AS "Razón Social",
		T1."U_EXP_NROSUNAT" AS "Nro. de Documento",
		TO_NVARCHAR(T4."TaxDate",'YYYYMMDD') AS "Fecha Emisión Documento",
		REPLACE(REPLACE(CAST(CAST(T1."U_EXP_IMPORTE" AS DECIMAL(18,2)) AS NVARCHAR(11)),',',''),'.','') AS "Importe del Documento",
		CASE T1."U_EXP_MEDIODEPAGO"
			WHEN 'CG' THEN '1'
			WHEN 'TB' THEN
				CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
					WHEN '009' THEN (SELECT 
										case "UsrNumber2" 
											when 'C' then '2'  
											when 'A' then '3'
											else '1' end
									FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
					ELSE '4' END
			ELSE replicate(' ',1) END AS "Forma de Pago",
		CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN
			CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
				WHEN '009' THEN LEFT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),3)
				ELSE replicate(' ',3) END
			ELSE '' END AS "Oficina de Cuenta Abono SBP",
		CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN
			CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
				WHEN '009' THEN RIGHT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),LENGTH(REPLACE(T1."U_EXP_NROCTAPROV",'-',''))-3)
				ELSE replicate(' ',7) END
			ELSE '' END AS "Cuenta Abono SBP",
		CASE T1."U_EXP_MEDIODEPAGO"
			WHEN 'CG' THEN '*S'
			ELSE 'N' END AS "Pago Único",
		IFNULL(T3."E_Mail",'') AS "Email de Proveedor",
		CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
			WHEN '009' THEN replicate(' ',20)
			ELSE REPLACE(T1."U_EXP_NROCTAPROV",'-','') END AS "CCI",
		CASE T1."U_EXP_MONEDA" WHEN 'SOL' THEN '00' WHEN 'USD' THEN '01' ELSE '' END AS "Moneda",
		'01' AS "Tipo de Pago"
		
		FROM "@EXP_OPMP" T0
		INNER JOIN "@EXP_PMP1" T1 ON T0."DocEntry"=T1."DocEntry"
		INNER JOIN DSC1 T2 ON T1."U_EXP_CODCTABANCO"=T2."GLAccount" and T2."Branch" = T1."U_EXP_COD_SUCURSAL"
		INNER JOIN OCRD T3 ON T1."U_EXP_CARDCODE"=T3."CardCode"
		INNER JOIN OPCH T4 ON T4."ObjType"=T1."U_EXP_TIPODOC" AND T4."DocEntry"=T1."U_EXP_DOCENTRYDOC"
		WHERE T0."DocEntry"=:docEntry AND T1."U_EXP_MEDIODEPAGO" IN ('TB','CG')
		AND T1."U_EXP_CODBANCO"='009' 
		AND T1."U_EXP_CODCTABANCO"=:glaccount
		AND T1."U_EXP_COD_SUCURSAL"=:codSucursal
		--AND T4."Indicator" IN ('00','01','08','SA') AND T3."U_EXX_TIPODOCU"='6'
		AND T4."Indicator" IN ('00','01','02','08','14','50','99','05','91','SA') AND T3."U_EXX_TIPODOCU"='6'
		--('00','01','02','08','14','50','99','05','91','SA')
		AND ((T1."U_EXP_MEDIODEPAGO"='TB' AND IFNULL(T1."U_EXP_NROCTAPROV",'')!='') OR (T1."U_EXP_MEDIODEPAGO"='CG'))
		AND T1."U_EXP_SLC_RETENCION"='N'
		
		UNION ALL
		
		----- ODPO - PROVEEDORES -----
		SELECT
		CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN	
			CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
				WHEN '' THEN T3."LicTradNum"
				ELSE (
					SELECT C1."U_EXX_NUMDOC"
					FROM OCRB C0
					INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name" AND C1."U_EXC_BENEFI"='Y')
				END
			ELSE
				CASE T3."QryGroup26"
					WHEN 'Y' THEN :RUC
					ELSE CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
							WHEN '' THEN T3."LicTradNum"
							ELSE (
								SELECT C1."U_EXX_NUMDOC"
								FROM OCRB C0
								INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name" AND C1."U_EXC_BENEFI"='Y')
							END
					END
			END AS "RUC",
		CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."CardName"
			ELSE (
				SELECT C1."FirstName"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name" AND C1."U_EXC_BENEFI"='Y')
			END AS "Razón Social",
		T1."U_EXP_NROSUNAT" AS "Nro. de Documento",
		TO_NVARCHAR(T4."TaxDate",'YYYYMMDD') AS "Fecha Emisión Documento",
		REPLACE(REPLACE(CAST(CAST(T1."U_EXP_IMPORTE" AS DECIMAL(18,2)) AS NVARCHAR(11)),',',''),'.','') AS "Importe del Documento",
		CASE T1."U_EXP_MEDIODEPAGO"
			WHEN 'CG' THEN '1'
			WHEN 'TB' THEN
				CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
					WHEN '009' THEN (SELECT 
										case "UsrNumber2" 
											when 'C' then '2'  
											when 'A' then '3'
											else '1' end 
									FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
					ELSE '4' END
			ELSE replicate(' ',1) END AS "Forma de Pago",
		CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN
			CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
				WHEN '009' THEN LEFT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),3)
				ELSE replicate(' ',3) END
			ELSE '' END AS "Oficina de Cuenta Abono SBP",
		CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN
			CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
				WHEN '009' THEN RIGHT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),LENGTH(REPLACE(T1."U_EXP_NROCTAPROV",'-',''))-3)
				ELSE replicate(' ',7) END
			ELSE '' END AS "Cuenta Abono SBP",
		CASE T1."U_EXP_MEDIODEPAGO"
			WHEN 'CG' THEN '*S'
			ELSE 'N' END AS "Pago Único",
		IFNULL(T3."E_Mail",'') AS "Email de Proveedor",
		CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
			WHEN '009' THEN replicate(' ',20)
			ELSE REPLACE(T1."U_EXP_NROCTAPROV",'-','') END AS "CCI",
		CASE T1."U_EXP_MONEDA" WHEN 'SOL' THEN '00' WHEN 'USD' THEN '01' ELSE '' END AS "Moneda",
		'01' AS "Tipo de Pago"
		
		FROM "@EXP_OPMP" T0
		INNER JOIN "@EXP_PMP1" T1 ON T0."DocEntry"=T1."DocEntry"
		INNER JOIN DSC1 T2 ON T1."U_EXP_CODCTABANCO"=T2."GLAccount" and T2."Branch" = T1."U_EXP_COD_SUCURSAL"
		INNER JOIN OCRD T3 ON T1."U_EXP_CARDCODE"=T3."CardCode"
		INNER JOIN ODPO T4 ON T4."ObjType"=T1."U_EXP_TIPODOC" AND T4."DocEntry"=T1."U_EXP_DOCENTRYDOC"
		WHERE T0."DocEntry"=:docEntry AND T1."U_EXP_MEDIODEPAGO" IN ('TB','CG')
		AND T1."U_EXP_CODBANCO"='009' 
		AND T1."U_EXP_CODCTABANCO"=:glaccount
		AND T1."U_EXP_COD_SUCURSAL"=:codSucursal
		--AND T4."Indicator" IN ('00','01','08','SA') AND T3."U_EXX_TIPODOCU"='6'
		AND T4."Indicator" IN ('00','01','02','08','14','50','99','05','91','SA') AND T3."U_EXX_TIPODOCU"='6'
		
		AND ((T1."U_EXP_MEDIODEPAGO"='TB' AND IFNULL(T1."U_EXP_NROCTAPROV",'')!='') OR (T1."U_EXP_MEDIODEPAGO"='CG'))
		AND T1."U_EXP_SLC_RETENCION"='N'
		
		UNION ALL
		
		----- ORIN - PROVEEDORES -----
		SELECT
		CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN	
			CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
				WHEN '' THEN T3."LicTradNum"
				ELSE (
					SELECT C1."U_EXX_NUMDOC"
					FROM OCRB C0
					INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name" AND C1."U_EXC_BENEFI"='Y')
				END
			ELSE
				CASE T3."QryGroup26"
					WHEN 'Y' THEN :RUC
					ELSE CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
							WHEN '' THEN T3."LicTradNum"
							ELSE (
								SELECT C1."U_EXX_NUMDOC"
								FROM OCRB C0
								INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name" AND C1."U_EXC_BENEFI"='Y')
							END
					END
			END AS "RUC",
		CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."CardName"
			ELSE (
				SELECT C1."FirstName"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name" AND C1."U_EXC_BENEFI"='Y')
			END AS "Razón Social",
		T1."U_EXP_NROSUNAT" AS "Nro. de Documento",
		TO_NVARCHAR(T4."TaxDate",'YYYYMMDD') AS "Fecha Emisión Documento",
		REPLACE(REPLACE(CAST(CAST(T1."U_EXP_IMPORTE" AS DECIMAL(18,2)) AS NVARCHAR(11)),',',''),'.','') AS "Importe del Documento",
		CASE T1."U_EXP_MEDIODEPAGO"
			WHEN 'CG' THEN '1'
			WHEN 'TB' THEN
				CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
					WHEN '009' THEN (SELECT 										
										case "UsrNumber2" 
											when 'C' then '2'  
											when 'A' then '3'
											else '1' 
										end 
									FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
					ELSE '4' END
			ELSE replicate(' ',1) END AS "Forma de Pago",
		CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN
			CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
				WHEN '009' THEN LEFT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),3)
				ELSE replicate(' ',3) END
			ELSE '' END AS "Oficina de Cuenta Abono SBP",
		CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN
			CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
				WHEN '009' THEN RIGHT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),LENGTH(REPLACE(T1."U_EXP_NROCTAPROV",'-',''))-3)
				ELSE replicate(' ',7) END
			ELSE '' END AS "Cuenta Abono SBP",
		CASE T1."U_EXP_MEDIODEPAGO"
			WHEN 'CG' THEN '*S'
			ELSE 'N' END AS "Pago Único",
		IFNULL(T3."E_Mail",'') AS "Email de Proveedor",
		CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
			WHEN '009' THEN replicate(' ',20)
			ELSE REPLACE(T1."U_EXP_NROCTAPROV",'-','') END AS "CCI",
		CASE T1."U_EXP_MONEDA" WHEN 'SOL' THEN '00' WHEN 'USD' THEN '01' ELSE '' END AS "Moneda",
		'01' AS "Tipo de Pago"
		
		FROM "@EXP_OPMP" T0
		INNER JOIN "@EXP_PMP1" T1 ON T0."DocEntry"=T1."DocEntry"
		INNER JOIN DSC1 T2 ON T1."U_EXP_CODCTABANCO"=T2."GLAccount" and T2."Branch" = T1."U_EXP_COD_SUCURSAL"
		INNER JOIN OCRD T3 ON T1."U_EXP_CARDCODE"=T3."CardCode"
		INNER JOIN ORIN T4 ON T4."ObjType"=T1."U_EXP_TIPODOC" AND T4."DocEntry"=T1."U_EXP_DOCENTRYDOC"
		WHERE T0."DocEntry"=:docEntry AND T1."U_EXP_MEDIODEPAGO" IN ('TB','CG')
		AND T1."U_EXP_CODBANCO"='009' 
		AND T1."U_EXP_CODCTABANCO"=:glaccount
		AND T1."U_EXP_COD_SUCURSAL"=:codSucursal
		AND T4."Indicator" IN ('00','07') AND T3."U_EXX_TIPODOCU"='6'
		AND ((T1."U_EXP_MEDIODEPAGO"='TB' AND IFNULL(T1."U_EXP_NROCTAPROV",'')!='') OR (T1."U_EXP_MEDIODEPAGO"='CG'))
		AND T1."U_EXP_SLC_RETENCION"='N'
		
		UNION ALL
		
		----- PAGOS EFECTUADOS PRELIMINARES / ASIENTOS -----
		SELECT
		CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN	
			CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
				WHEN '' THEN T3."LicTradNum"
				ELSE (
					SELECT C1."U_EXX_NUMDOC"
					FROM OCRB C0
					INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name" AND C1."U_EXC_BENEFI"='Y')
				END
			ELSE
				CASE T3."QryGroup26"
					WHEN 'Y' THEN :RUC
					ELSE CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
							WHEN '' THEN T3."LicTradNum"
							ELSE (
								SELECT C1."U_EXX_NUMDOC"
								FROM OCRB C0
								INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name" AND C1."U_EXC_BENEFI"='Y')
							END
					END
			END AS "RUC",
		CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."CardName"
			ELSE (
				SELECT C1."FirstName"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name" AND C1."U_EXC_BENEFI"='Y')
			END AS "Razón Social",
		T1."U_EXP_NROSUNAT" AS "Nro. de Documento",
		TO_NVARCHAR(T0."U_EXP_FECHA",'YYYYMMDD') AS "Fecha Emisión Documento",
		REPLACE(REPLACE(CAST(CAST(T1."U_EXP_IMPORTE" AS DECIMAL(18,2)) AS NVARCHAR(11)),',',''),'.','') AS "Importe del Documento",
		CASE T1."U_EXP_MEDIODEPAGO"
			WHEN 'CG' THEN '1'
			WHEN 'TB' THEN
				CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
					WHEN '009' THEN (SELECT 										
										case "UsrNumber2" 
											when 'C' then '2'  
											when 'A' then '3'
											else '1' 
										end 
									FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
					ELSE '4' END
			ELSE replicate(' ',1) END AS "Forma de Pago",
		CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN
			CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
				WHEN '009' THEN LEFT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),3)
				ELSE replicate(' ',3) END
			ELSE '' END AS "Oficina de Cuenta Abono SBP",
		CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN
			CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
				WHEN '009' THEN RIGHT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),LENGTH(REPLACE(T1."U_EXP_NROCTAPROV",'-',''))-3)
				ELSE replicate(' ',7) END
			ELSE '' END AS "Cuenta Abono SBP",
		CASE T1."U_EXP_MEDIODEPAGO"
			WHEN 'CG' THEN '*S'
			ELSE 'N' END AS "Pago Único",
		IFNULL(T3."E_Mail",'') AS "Email de Proveedor",
		CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
			WHEN '009' THEN replicate(' ',20)
			ELSE REPLACE(T1."U_EXP_NROCTAPROV",'-','') END AS "CCI",
		CASE T1."U_EXP_MONEDA" WHEN 'SOL' THEN '00' WHEN 'USD' THEN '01' ELSE '' END AS "Moneda",
		'01' AS "Tipo de Pago"
		
		FROM "@EXP_OPMP" T0
		INNER JOIN "@EXP_PMP1" T1 ON T0."DocEntry"=T1."DocEntry"
		INNER JOIN DSC1 T2 ON T1."U_EXP_CODCTABANCO"=T2."GLAccount" and T2."Branch" = T1."U_EXP_COD_SUCURSAL"
		INNER JOIN OCRD T3 ON T1."U_EXP_CARDCODE"=T3."CardCode"
		WHERE T0."DocEntry"=:docEntry AND T1."U_EXP_MEDIODEPAGO" IN ('TB','CG')
		AND T1."U_EXP_CODBANCO"='009' 
		AND T1."U_EXP_CODCTABANCO"=:glaccount
		AND T1."U_EXP_COD_SUCURSAL"=:codSucursal
		AND T1."U_EXP_TIPODOC" IN ('30','46','140') --AND T3."U_EXX_TIPODOCU"='6'
		AND ((T1."U_EXP_MEDIODEPAGO"='TB' AND IFNULL(T1."U_EXP_NROCTAPROV",'')!='') OR (T1."U_EXP_MEDIODEPAGO"='CG'))
		AND T1."U_EXP_SLC_RETENCION"='N'
	) A0;

	IF :formatoScotia = 'PV' THEN	
		select 
			LEFT("RUC"||replicate(' ',11),11)||
			LEFT("Razón Social"||replicate(' ',60),60)||
			LEFT("Nro. de Documento"||replicate(' ',14),14)||
			LEFT("Fecha Emisión Documento"||replicate(' ',8),8)||
			RIGHT(replicate('0',11)||"Importe del Documento",11)||
			LEFT(ifnull("Forma de Pago",'')||replicate(' ',1),1)||
			LEFT("Oficina de Cuenta Abono SBP"||replicate(' ',3),3)||
			LEFT("Cuenta Abono SBP"||replicate(' ',7),7)||
			LEFT(/*A0."Pago Único"*/''||replicate(' ',1),2)||
			LEFT("Email de Proveedor"||replicate(' ',50),50)--||
			--LEFT(A0."CCI"||replicate(' ',20),20)||
			--LEFT(A0."Moneda"||replicate(' ',2),2)||
			--LEFT(A0."Tipo de Pago"||replicate(' ',2),2)
			||'|'
				AS "Resultado"
		from :RSLT;	
	ELSEIF :formatoScotia = 'HB' THEN	
		select 
			LEFT(LEFT("RUC",8)||replicate(' ',8),8)||
			LEFT(LEFT("Razón Social",30)||replicate(' ',30),30)||
			LEFT('ENTREGAS A RENDIR'||replicate(' ',20),20)||
			LEFT("Fecha Emisión Documento"||replicate(' ',8),8)||
			RIGHT(replicate('0',11)||"Importe del Documento",11)||
			LEFT(ifnull("Forma de Pago",'')||replicate(' ',1),1)||
			LEFT("Oficina de Cuenta Abono SBP"||replicate(' ',3),3)||
			LEFT("Cuenta Abono SBP"||replicate(' ',7),7)||
			LEFT(LEFT("RUC",8)||replicate(' ',8),8)	
			--LEFT(A0."Moneda"||replicate(' ',2),2)||
			--LEFT(A0."Tipo de Pago"||replicate(' ',2),2)
			||'|'
				AS "Resultado"
		from :RSLT;	
	END IF;

ELSEIF :factoring='Y' THEN
-- Estructura Orden de Pago Factoring
	SELECT
	LEFT(A2."RUC"||replicate(' ',11),11)|| -- posición de 1 a 10
	LEFT(A2."Razón Social"||replicate(' ',60),60)|| -- posición de 12 a 71
	LEFT(A2."Nro. de Documento"||replicate(' ',14),14)|| -- posición de 72 a 85
	LEFT(A2."Fecha Emisión Documento"||replicate(' ',8),8)|| -- posición de 86 a 93
	RIGHT(replicate('0',11)||A2."Importe del Documento",11)|| -- posición de 94 a 104
	LEFT(A2."Email de Proveedor"||replicate(' ',50),50)|| -- posición de 105 a 154
	LEFT(A2."Fecha Vencimiento Documento"||replicate(' ',8),8)|| -- posición de 155 a 162
	LEFT(A2."Moneda"||replicate(' ',2),2)|| -- posición de 163 a 164
	LEFT(A2."Tipo de Pago"||replicate(' ',2),2) -- posición de 165 a 166
	||'|'
		AS "Resultado"
	
	FROM (
		----- OPCH - PROVEEDORES -----
		SELECT
		CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN	
			CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
				WHEN '' THEN T3."LicTradNum"
				ELSE (
					SELECT C1."U_EXX_NUMDOC"
					FROM OCRB C0
					INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name" AND C1."U_EXC_BENEFI"='Y')
				END
			ELSE
				CASE T3."QryGroup26"
					WHEN 'Y' THEN :RUC
					ELSE CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
							WHEN '' THEN T3."LicTradNum"
							ELSE (
								SELECT C1."U_EXX_NUMDOC"
								FROM OCRB C0
								INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name" AND C1."U_EXC_BENEFI"='Y')
							END
					END
			END AS "RUC",
		CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."CardName"
			ELSE (
				SELECT C1."FirstName"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name" AND C1."U_EXC_BENEFI"='Y')
			END AS "Razón Social",
		T4."NumAtCard" AS "Nro. de Documento",
		TO_NVARCHAR(T4."TaxDate",'YYYYMMDD') AS "Fecha Emisión Documento",
		TO_NVARCHAR(T5."DueDate",'YYYYMMDD') AS "Fecha Vencimiento Documento",
		REPLACE(REPLACE(CAST(CAST(T1."U_EXP_IMPORTE" AS DECIMAL(18,2)) AS NVARCHAR(11)),',',''),'.','') AS "Importe del Documento",
		CASE T1."U_EXP_MEDIODEPAGO"
			WHEN 'CG' THEN '1'
			WHEN 'TB' THEN
				CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
					WHEN '009' THEN (SELECT "UsrNumber2" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
					ELSE '4' END
			ELSE replicate(' ',1) END AS "Forma de Pago",
		CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN
			CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
				WHEN '009' THEN LEFT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),3)
				ELSE replicate(' ',3) END
			ELSE '' END AS "Oficina de Cuenta Abono SBP",
		CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN
			CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
				WHEN '009' THEN RIGHT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),LENGTH(REPLACE(T1."U_EXP_NROCTAPROV",'-',''))-3)
				ELSE replicate(' ',7) END
			ELSE '' END AS "Cuenta Abono SBP",
		CASE T1."U_EXP_MEDIODEPAGO"
			WHEN 'CG' THEN '*S'
			ELSE 'N' END AS "Pago Único",
		IFNULL(T3."E_Mail",'') AS "Email de Proveedor",
		CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
			WHEN '009' THEN replicate(' ',20)
			ELSE REPLACE(T1."U_EXP_NROCTAPROV",'-','') END AS "CCI",
		CASE T1."U_EXP_MONEDA" WHEN 'SOL' THEN '00' WHEN 'USD' THEN '01' ELSE '' END AS "Moneda",
		'10' AS "Tipo de Pago"
		
		FROM "@EXP_OPMP" T0
		INNER JOIN "@EXP_PMP1" T1 ON T0."DocEntry"=T1."DocEntry"
		INNER JOIN DSC1 T2 ON T1."U_EXP_CODCTABANCO"=T2."GLAccount" and T2."Branch" = T1."U_EXP_COD_SUCURSAL"
		INNER JOIN OCRD T3 ON T1."U_EXP_CARDCODE"=T3."CardCode"
		INNER JOIN OPCH T4 ON T4."ObjType"=T1."U_EXP_TIPODOC" AND T4."DocEntry"=T1."U_EXP_DOCENTRYDOC"
		INNER JOIN PCH6 T5 ON T4."DocEntry"=T5."DocEntry" AND T1."U_EXP_NMROCUOTA"=T5."InstlmntID"
		WHERE T0."DocEntry"=:docEntry AND T1."U_EXP_MEDIODEPAGO" IN ('TB','CG')
		AND T1."U_EXP_CODBANCO"='009' 
		AND T1."U_EXP_CODCTABANCO"=:glaccount
		AND T1."U_EXP_COD_SUCURSAL"=:codSucursal
		AND T4."Indicator" IN ('00','01','02','08','14','50','99','05','91','SA') AND T3."U_EXX_TIPODOCU"='6'
		AND ((T1."U_EXP_MEDIODEPAGO"='TB') OR (T1."U_EXP_MEDIODEPAGO"='CG'))
		AND T1."U_EXP_SLC_RETENCION"='N'
		
		UNION ALL
		
		----- ODPO - PROVEEDORES -----
		SELECT
		CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN	
			CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
				WHEN '' THEN T3."LicTradNum"
				ELSE (
					SELECT C1."U_EXX_NUMDOC"
					FROM OCRB C0
					INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name" AND C1."U_EXC_BENEFI"='Y')
				END
			ELSE
				CASE T3."QryGroup26"
					WHEN 'Y' THEN :RUC
					ELSE CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
							WHEN '' THEN T3."LicTradNum"
							ELSE (
								SELECT C1."U_EXX_NUMDOC"
								FROM OCRB C0
								INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name" AND C1."U_EXC_BENEFI"='Y')
							END
					END
			END AS "RUC",
		CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."CardName"
			ELSE (
				SELECT C1."FirstName"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name" AND C1."U_EXC_BENEFI"='Y')
			END AS "Razón Social",
		T4."NumAtCard" AS "Nro. de Documento",
		TO_NVARCHAR(T4."TaxDate",'YYYYMMDD') AS "Fecha Emisión Documento",
		TO_NVARCHAR(T4."DocDueDate",'YYYYMMDD') AS "Fecha Vencimiento Documento",
		REPLACE(REPLACE(CAST(CAST(T1."U_EXP_IMPORTE" AS DECIMAL(18,2)) AS NVARCHAR(11)),',',''),'.','') AS "Importe del Documento",
		CASE T1."U_EXP_MEDIODEPAGO"
			WHEN 'CG' THEN '1'
			WHEN 'TB' THEN
				CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
					WHEN '009' THEN (SELECT "UsrNumber2" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
					ELSE '4' END
			ELSE replicate(' ',1) END AS "Forma de Pago",
		CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN
			CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
				WHEN '009' THEN LEFT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),3)
				ELSE replicate(' ',3) END
			ELSE '' END AS "Oficina de Cuenta Abono SBP",
		CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN
			CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
				WHEN '009' THEN RIGHT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),LENGTH(REPLACE(T1."U_EXP_NROCTAPROV",'-',''))-3)
				ELSE replicate(' ',7) END
			ELSE '' END AS "Cuenta Abono SBP",
		CASE T1."U_EXP_MEDIODEPAGO"
			WHEN 'CG' THEN '*S'
			ELSE 'N' END AS "Pago Único",
		IFNULL(T3."E_Mail",'') AS "Email de Proveedor",
		CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
			WHEN '009' THEN replicate(' ',20)
			ELSE REPLACE(T1."U_EXP_NROCTAPROV",'-','') END AS "CCI",
		CASE T1."U_EXP_MONEDA" WHEN 'SOL' THEN '00' WHEN 'USD' THEN '01' ELSE '' END AS "Moneda",
		'10' AS "Tipo de Pago"
		
		FROM "@EXP_OPMP" T0
		INNER JOIN "@EXP_PMP1" T1 ON T0."DocEntry"=T1."DocEntry"
		INNER JOIN DSC1 T2 ON T1."U_EXP_CODCTABANCO"=T2."GLAccount" and T2."Branch" = T1."U_EXP_COD_SUCURSAL"
		INNER JOIN OCRD T3 ON T1."U_EXP_CARDCODE"=T3."CardCode"
		INNER JOIN ODPO T4 ON T4."ObjType"=T1."U_EXP_TIPODOC" AND T4."DocEntry"=T1."U_EXP_DOCENTRYDOC"
		WHERE T0."DocEntry"=:docEntry AND T1."U_EXP_MEDIODEPAGO" IN ('TB','CG')
		AND T1."U_EXP_CODBANCO"='009' 
		AND T1."U_EXP_CODCTABANCO"=:glaccount
		AND T1."U_EXP_COD_SUCURSAL"=:codSucursal
		AND T4."Indicator" IN ('00','01','02','08','14','50','99','05','91','SA') AND T3."U_EXX_TIPODOCU"='6'
		
		AND ((T1."U_EXP_MEDIODEPAGO"='TB' ) OR (T1."U_EXP_MEDIODEPAGO"='CG'))
		AND T1."U_EXP_SLC_RETENCION"='N'
		
		UNION ALL
		
		----- ORIN - PROVEEDORES -----
		SELECT
		CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN	
			CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
				WHEN '' THEN T3."LicTradNum"
				ELSE (
					SELECT C1."U_EXX_NUMDOC"
					FROM OCRB C0
					INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name" AND C1."U_EXC_BENEFI"='Y')
				END
			ELSE
				CASE T3."QryGroup26"
					WHEN 'Y' THEN :RUC
					ELSE CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
							WHEN '' THEN T3."LicTradNum"
							ELSE (
								SELECT C1."U_EXX_NUMDOC"
								FROM OCRB C0
								INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name" AND C1."U_EXC_BENEFI"='Y')
							END
					END
			END AS "RUC",
		CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."CardName"
			ELSE (
				SELECT C1."FirstName"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name" AND C1."U_EXC_BENEFI"='Y')
			END AS "Razón Social",
		IFNULL(T4."NumAtCard",IFNULL(T4."FolioPref",'NC01')||'-'||IFNULL(T4."FolioNum", T4. "DocNum")) AS "Nro. de Documento",
		TO_NVARCHAR(T4."TaxDate",'YYYYMMDD') AS "Fecha Emisión Documento",
		TO_NVARCHAR(T4."DocDueDate",'YYYYMMDD') AS "Fecha Vencimiento Documento",
		REPLACE(REPLACE(CAST(CAST(T1."U_EXP_IMPORTE" AS DECIMAL(18,2)) AS NVARCHAR(11)),',',''),'.','') AS "Importe del Documento",
		CASE T1."U_EXP_MEDIODEPAGO"
			WHEN 'CG' THEN '1'
			WHEN 'TB' THEN
				CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
					WHEN '009' THEN (SELECT "UsrNumber2" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
					ELSE '4' END
			ELSE replicate(' ',1) END AS "Forma de Pago",
		CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN
			CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
				WHEN '009' THEN LEFT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),3)
				ELSE replicate(' ',3) END
			ELSE '' END AS "Oficina de Cuenta Abono SBP",
		CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN
			CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
				WHEN '009' THEN RIGHT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),LENGTH(REPLACE(T1."U_EXP_NROCTAPROV",'-',''))-3)
				ELSE replicate(' ',7) END
			ELSE '' END AS "Cuenta Abono SBP",
		CASE T1."U_EXP_MEDIODEPAGO"
			WHEN 'CG' THEN '*S'
			ELSE 'N' END AS "Pago Único",
		IFNULL(T3."E_Mail",'') AS "Email de Proveedor",
		CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
			WHEN '009' THEN replicate(' ',20)
			ELSE REPLACE(T1."U_EXP_NROCTAPROV",'-','') END AS "CCI",
		CASE T1."U_EXP_MONEDA" WHEN 'SOL' THEN '00' WHEN 'USD' THEN '01' ELSE '' END AS "Moneda",
		'10' AS "Tipo de Pago"
		
		FROM "@EXP_OPMP" T0
		INNER JOIN "@EXP_PMP1" T1 ON T0."DocEntry"=T1."DocEntry"
		INNER JOIN DSC1 T2 ON T1."U_EXP_CODCTABANCO"=T2."GLAccount"
		INNER JOIN OCRD T3 ON T1."U_EXP_CARDCODE"=T3."CardCode"
		INNER JOIN ORIN T4 ON T4."ObjType"=T1."U_EXP_TIPODOC" AND T4."DocEntry"=T1."U_EXP_DOCENTRYDOC"
		WHERE T0."DocEntry"=:docEntry AND T1."U_EXP_MEDIODEPAGO" IN ('TB','CG')
		AND T1."U_EXP_CODBANCO"='009' 
		AND T1."U_EXP_CODCTABANCO"=:glaccount
		AND T1."U_EXP_COD_SUCURSAL"=:codSucursal
		AND T4."Indicator" IN ('00','07') AND T3."U_EXX_TIPODOCU"='6'
		AND ((T1."U_EXP_MEDIODEPAGO"='TB' ) OR (T1."U_EXP_MEDIODEPAGO"='CG'))
		AND T1."U_EXP_SLC_RETENCION"='N'
		
		UNION ALL
		
		----- PAGOS EFECTUADOS PRELIMINARES / ASIENTOS -----
		SELECT
		CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN	
			CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
				WHEN '' THEN T3."LicTradNum"
				ELSE (
					SELECT C1."U_EXX_NUMDOC"
					FROM OCRB C0
					INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name" AND C1."U_EXC_BENEFI"='Y')
				END
			ELSE
				CASE T3."QryGroup26"
					WHEN 'Y' THEN :RUC
					ELSE CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
							WHEN '' THEN T3."LicTradNum"
							ELSE (
								SELECT C1."U_EXX_NUMDOC"
								FROM OCRB C0
								INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name" AND C1."U_EXC_BENEFI"='Y')
							END
					END
			END AS "RUC",
		CASE IFNULL((SELECT IFNULL("U_EXC_BENEFI",'') FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV"),'')
			WHEN '' THEN T3."CardName"
			ELSE (
				SELECT C1."FirstName"
				FROM OCRB C0
				INNER JOIN OCPR C1 ON C0."CardCode"=C1."CardCode" AND C0."CardCode"=T3."CardCode" AND C0."U_EXC_BENEFI"=C1."Name" AND C1."U_EXC_BENEFI"='Y')
			END AS "Razón Social",
		T1."U_EXP_DOCENTRYDOC" AS "Nro. de Documento",
		TO_NVARCHAR(T0."U_EXP_FECHA",'YYYYMMDD') AS "Fecha Emisión Documento",
		TO_NVARCHAR(T0."U_EXP_FECHA",'YYYYMMDD') AS "Fecha Vencimiento Documento",
		REPLACE(REPLACE(CAST(CAST(T1."U_EXP_IMPORTE" AS DECIMAL(18,2)) AS NVARCHAR(11)),',',''),'.','') AS "Importe del Documento",
		CASE T1."U_EXP_MEDIODEPAGO"
			WHEN 'CG' THEN '1'
			WHEN 'TB' THEN
				CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
					WHEN '009' THEN (SELECT 
										case "UsrNumber2" 
											when 'C' then '2'  
											when 'A' then '3'
											else '1' end
									FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
					ELSE '4' END
			ELSE replicate(' ',1) END AS "Forma de Pago",
		CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN
			CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
				WHEN '009' THEN LEFT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),3)
				ELSE replicate(' ',3) END
			ELSE '' END AS "Oficina de Cuenta Abono SBP",
		CASE T1."U_EXP_MEDIODEPAGO" WHEN 'TB' THEN
			CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
				WHEN '009' THEN RIGHT(REPLACE(T1."U_EXP_NROCTAPROV",'-',''),LENGTH(REPLACE(T1."U_EXP_NROCTAPROV",'-',''))-3)
				ELSE replicate(' ',7) END
			ELSE '' END AS "Cuenta Abono SBP",
		CASE T1."U_EXP_MEDIODEPAGO"
			WHEN 'CG' THEN '*S'
			ELSE 'N' END AS "Pago Único",
		IFNULL(T3."E_Mail",'') AS "Email de Proveedor",
		CASE (SELECT "BankCode" FROM OCRB WHERE "CardCode" = T1."U_EXP_CARDCODE" AND "Account"=T1."U_EXP_NROCTAPROV")
			WHEN '009' THEN replicate(' ',20)
			ELSE REPLACE(T1."U_EXP_NROCTAPROV",'-','') END AS "CCI",
		CASE T1."U_EXP_MONEDA" WHEN 'SOL' THEN '00' WHEN 'USD' THEN '01' ELSE '' END AS "Moneda",
		'10' AS "Tipo de Pago"
		
		FROM "@EXP_OPMP" T0
		INNER JOIN "@EXP_PMP1" T1 ON T0."DocEntry"=T1."DocEntry"
		INNER JOIN DSC1 T2 ON T1."U_EXP_CODCTABANCO"=T2."GLAccount" and T2."Branch" = T1."U_EXP_COD_SUCURSAL"
		INNER JOIN OCRD T3 ON T1."U_EXP_CARDCODE"=T3."CardCode"
		WHERE T0."DocEntry"=:docEntry AND T1."U_EXP_MEDIODEPAGO" IN ('TB','CG')
		AND T1."U_EXP_CODBANCO"='009' 
		AND T1."U_EXP_CODCTABANCO"=:glaccount
		AND T1."U_EXP_COD_SUCURSAL"=:codSucursal
		AND T1."U_EXP_TIPODOC" IN ('30','46','140') AND T3."U_EXX_TIPODOCU"='6'
		AND ((T1."U_EXP_MEDIODEPAGO"='TB' ) OR (T1."U_EXP_MEDIODEPAGO"='CG'))
		AND T1."U_EXP_SLC_RETENCION"='N'
	) A2;

END IF;

END;