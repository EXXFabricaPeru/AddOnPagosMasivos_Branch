﻿CREATE PROCEDURE "SMC_APM_LISTAR_FACPENDIENTES_PP"
	(fechaVenc DATE,
	moneda VARCHAR(3),
	CardCode VARCHAR(20),
	escenario varchar(15),
	tipoBanco varchar(3),
	filtroBanco varchar(3))
AS
BEGIN
	SELECT 
		ROW_NUMBER() OVER(ORDER BY T0."FechaVencimiento" DESC) AS "FILA",
		T0."DocEntry",
		T0."DocNum",
		T0."FechaContable",
		T0."FechaVencimiento",
		T0."CardCode",
		T0."CardName",
		T0."NumAtCard",
		T0."DocCur",
		T0."Total",
		T0."CodigoRetencion",
		T0."Retencion",
		T0."TotalPagar",
		T0."RUC",
		T0."Cuenta",
		T0."CuentaMoneda",
		T0."BankCode",
		T0."Atraso",
		T0."Marca",
		T0."Estado",
		T0."Documento",
		(select MAX("BankName")as "BankName" from ODSC where "BankCode" = T0."BankCode") as "NombreBanco",
		T0."Origen",
		
		/*
		T0."Comentario",
		T0."Propiedad",
		T0."DueDate",
		*/
		T0."BloqueoPago",
		IFNULL(T0."DetraccionPend", 'N') AS "DetraccionPend"
		,"NroCuota"
		,"LineaAsiento"
	FROM 
		(SELECT
		T0."DocEntry",
		T0."DocNum",
		T0."TaxDate" AS "FechaContable",
		T1."DueDate" AS "FechaVencimiento",
		T0."CardCode",
		T0."CardName",
		T0."NumAtCard",
		T0."DocCur",
		CAST( 
		(CASE 
			WHEN T0."DocCur" in ('USD','EUR') 
				THEN T1."InsTotalFC" --- T1."PaidFC" + 
				--(SELECT (R0."InsTotalFC"- R0."PaidFC") FROM PCH6 R0 WHERE R0."DocEntry" = T0."DocEntry" AND R0."InstlmntID" = '1')
			ELSE 
				T1."InsTotal" --- T1."PaidToDate" --+ 
				--(SELECT (R0."InsTotal"- R0."PaidToDate") FROM PCH6 R0 WHERE R0."DocEntry" = T0."DocEntry" AND R0."InstlmntID" = '1')
		END)AS DECIMAL(16,2)) AS "Total",
		
		IFNULL(T5."OffclCode",'') AS "CodigoRetencion" ,
		
		CAST((CASE 
			WHEN T0."DocCur" in ('USD','EUR') THEN 
				T0."WTSumFC"
			ELSE 
				T0."WTSum"
		END)AS DECIMAL(16,2)) AS "Retencion",
		
		
		
		CAST((CASE 	WHEN T0."DocCur" in ('USD','EUR') THEN 
							CASE (SELECT "WTCode" FROM PCH5 WHERE "AbsEntry" = T0."DocEntry")
								WHEN 'RT4C' THEN T1."InsTotalFC" - T1."PaidFC"
								ELSE T1."InsTotalFC" - T1."PaidFC"--+ T0."WTSumFC" - T4."ApplAmntFC"
							END
						ELSE 
							CASE (SELECT "WTCode" FROM PCH5 WHERE "AbsEntry" = T0."DocEntry")
								WHEN 'RT4C' THEN T1."InsTotal" - T1."PaidToDate"
								ELSE T1."InsTotal" - T1."PaidToDate"-- + T0."WTSum" - T4."ApplAmnt"
							END
			END)AS DECIMAL(16,2)) AS "TotalPagar",
			
		T3."LicTradNum" AS "RUC",
		
		
		
		/*
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."Account") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" --*AND "BankCode" = :tipoBanco
			AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,'')*/
		(SELECT CASE T0."U_EXC_PAGBEN" WHEN 'Y' THEN R2."Account" ELSE R3."Account" END
		FROM OCRD R0
		LEFT JOIN OCPR R1 ON R0."CardCode"=R1."CardCode" AND R1."U_EXC_BENEFI"='Y'
		LEFT JOIN OCRB R2 ON R1."Name"=R2."U_EXC_BENEFI" AND IFNULL(R2."UsrNumber1",'') = :moneda --beneficiario
		LEFT JOIN OCRB R3 ON R0."CardCode"=R3."CardCode" AND R3."U_EXC_ACTIVO"='Y' AND IFNULL(R3."UsrNumber1",'') = :moneda --normal
		WHERE R0."CardCode" = T0."CardCode")
		AS "Cuenta",
		
		/*
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" --AND "BankCode" = :tipoBanco
			AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,'') */
		(SELECT CASE T0."U_EXC_PAGBEN" WHEN 'Y' THEN R2."UsrNumber1" ELSE R3."UsrNumber1" END
		FROM OCRD R0
		LEFT JOIN OCPR R1 ON R0."CardCode"=R1."CardCode" AND R1."U_EXC_BENEFI"='Y'
		LEFT JOIN OCRB R2 ON R1."Name"=R2."U_EXC_BENEFI" AND IFNULL(R2."UsrNumber1",'') = :moneda --beneficiario
		LEFT JOIN OCRB R3 ON R0."CardCode"=R3."CardCode" AND R3."U_EXC_ACTIVO"='Y' AND IFNULL(R3."UsrNumber1",'') = :moneda --normal
		WHERE R0."CardCode" = T0."CardCode")
		AS "CuentaMoneda",
		
		/*
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" --AND "BankCode" = :tipoBanco
			AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,'')*/
		(SELECT CASE T0."U_EXC_PAGBEN" WHEN 'Y' THEN R2."BankCode" ELSE R3."BankCode" END
		FROM OCRD R0
		LEFT JOIN OCPR R1 ON R0."CardCode"=R1."CardCode" AND R1."U_EXC_BENEFI"='Y'
		LEFT JOIN OCRB R2 ON R1."Name"=R2."U_EXC_BENEFI" AND IFNULL(R2."UsrNumber1",'') = :moneda --beneficiario
		LEFT JOIN OCRB R3 ON R0."CardCode"=R3."CardCode" AND R3."U_EXC_ACTIVO"='Y' AND IFNULL(R3."UsrNumber1",'') = :moneda --normal
		WHERE R0."CardCode" = T0."CardCode")
		AS "BankCode",
		
		(CASE 
			WHEN DAYS_BETWEEN(T0."DocDueDate", :fechaVenc) < 0 THEN 0 
			ELSE DAYS_BETWEEN(T0."DocDueDate", :fechaVenc) 
		END) AS "Atraso",
		--0 AS "Atraso",
		'' AS "Marca",
		'' AS "Estado",
		'FT-P' as "Documento",
		T0."Comments" AS "Comentario",
		CASE WHEN T3."QryGroup11" = 'Y' THEN 'PROV. CAJA CHICA' ELSE '' END AS "Propiedad",
		T1."DueDate"
		,ifnull(T0."U_EXC_ORIGEN",'') as "Origen"
		,IFNULL(T0."PayBlock", 'N') AS "BloqueoPago"
		,(SELECT CASE WHEN "InsTotal" <> "PaidToDate" THEN 'Y' ELSE 'N' END FROM PCH6 WHERE "DocEntry" = T0."DocEntry" AND UPPER(IFNULL("U_EXX_CONFTIPODET", 'No'))  = 'SI' ) AS "DetraccionPend"
			,T1."InstlmntID" AS "NroCuota"
		,0 AS "LineaAsiento"
	FROM 
		OPCH T0
		LEFT JOIN PCH6 T1 ON T0."DocEntry" = T1."DocEntry"
		LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
		LEFT JOIN OCRD T3 ON T0."CardCode" = T3."CardCode"
		LEFT JOIN PCH5 T4 ON T0."DocEntry" = T4."AbsEntry" AND T4."ObjType" = T0."ObjType"
		LEFT JOIN OWHT T5 ON T4."WTCode" = T5."WTCode"
		--LEFT JOIN OCRB T4 ON T0."CardCode" = T4."CardCode"
	WHERE 
		T0."Indicator" NOT IN ('DI','AN','AB')
		--T0."Indicator" IN ('01','02','03','08','DT','DO')
		AND T1."Status" = 'O'
		AND IFNULL(T1."U_EXX_CONFTIPODET",'No') ='No'
		AND T1."DueDate" <= :fechaVenc
		AND T0."DocCur" = UPPER(:moneda)
		AND T0."CardCode" like '%' || :CardCode || '%'
		AND T0."DocEntry" NOT IN (SELECT "U_SMC_DOCENTRY" FROM "@SMC_APM_ESCDET" 
									WHERE "U_SMC_ESCCAB" = :escenario and "U_SMC_TIPO_DOCUMENTO" = 'FT-P')
		--AND T0."DocTotal" NOT IN (SELECT "U_SMC_MONTO" FROM "@SMC_APM_ESCDET" WHERE "U_SMC_ESCCAB" = :escenario)
		
	UNION
	
	SELECT
		T0."DocEntry",
		T0."DocNum",
		T0."TaxDate" AS "FechaContable",
		T1."DueDate" AS "FechaVencimiento",
		T0."CardCode",
		T0."CardName",
		T0."NumAtCard",
		T0."DocCur",
		CAST( 
		(CASE 
			WHEN T0."DocCur" in ('USD','EUR') 
				THEN T1."InsTotalFC" --- T1."PaidFC" + 
				--(SELECT (R0."InsTotalFC"- R0."PaidFC") FROM PCH6 R0 WHERE R0."DocEntry" = T0."DocEntry" AND R0."InstlmntID" = '1')
			ELSE 
				T1."InsTotal" --- T1."PaidToDate" --+ 
				--(SELECT (R0."InsTotal"- R0."PaidToDate") FROM PCH6 R0 WHERE R0."DocEntry" = T0."DocEntry" AND R0."InstlmntID" = '1')
		END)AS DECIMAL(16,2)) AS "Total",
		
		IFNULL(T5."OffclCode",'') AS "CodigoRetencion" ,
		
		CAST((CASE 
			WHEN T0."DocCur" in ('USD','EUR') THEN 
				T0."WTSumFC"
			ELSE 
				T0."WTSum"
		END)AS DECIMAL(16,2)) AS "Retencion",
		CAST((CASE 	WHEN T0."DocCur" in ('USD','EUR') THEN 
							CASE (SELECT "WTCode" FROM RIN5 WHERE "AbsEntry" = T0."DocEntry")
								WHEN 'RT4C' THEN T1."InsTotalFC" - T1."PaidFC"
								ELSE T1."InsTotalFC" - T1."PaidFC"- T0."WTSumFC"
							END
						ELSE 
							CASE (SELECT "WTCode" FROM RIN5 WHERE "AbsEntry" = T0."DocEntry")
								WHEN 'RT4C' THEN T1."InsTotal" - T1."PaidToDate"
								ELSE T1."InsTotal" - T1."PaidToDate" - T0."WTSum"
							END
			END)AS DECIMAL(16,2)) AS "TotalPagar",
		T3."LicTradNum" AS "RUC",
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."Account") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" /*AND "BankCode" = :tipoBanco*/ AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,'')
		AS "Cuenta",
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" /*AND "BankCode" = :tipoBanco*/ AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,'') 
		AS "CuentaMoneda",
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" /*AND "BankCode" = :tipoBanco*/ AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,'')
		AS "BankCode",
		(CASE 
			WHEN DAYS_BETWEEN(T0."DocDueDate", :fechaVenc) < 0 THEN 0 
			ELSE DAYS_BETWEEN(T0."DocDueDate", :fechaVenc) 
		END) AS "Atraso",
		--0 AS "Atraso",
		'' AS "Marca",
		'' AS "Estado",
		'NC-C' as "Documento",
		T0."Comments" AS "Comentario",
		CASE WHEN T3."QryGroup11" = 'Y' THEN 'PROV. CAJA CHICA' ELSE '' END AS "Propiedad",
		T1."DueDate"
		,ifnull(T0."U_EXC_ORIGEN",'') as "Origen"
		,IFNULL(T0."PayBlock", 'N') AS "BloqueoPago"
		,'N' AS "DetraccionPend"
	    ,T1."InstlmntID" AS "NroCuota"
		,0 AS "LineaAsiento"
	FROM 
		ORIN T0
		LEFT JOIN RIN6 T1 ON T0."DocEntry" = T1."DocEntry"
		LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
		LEFT JOIN OCRD T3 ON T0."CardCode" = T3."CardCode"
		LEFT JOIN PCH5 T4 ON T0."DocEntry" = T4."AbsEntry" AND T4."ObjType" = T0."ObjType"
		LEFT JOIN OWHT T5 ON T4."WTCode" = T5."WTCode"
		--LEFT JOIN OCRB T4 ON T0."CardCode" = T4."CardCode"
	WHERE 
		T0."Indicator" IN ('00','01','02','08','14','50','99','91','05','07')
		--T0."Indicator" IN ('01','02','03','08','DT','DO')
		AND T1."InstlmntID" = '1'
		AND T1."Status" = 'O'
		AND T2."PymntGroup" not like '%DT%'
		AND T1."DueDate" <= :fechaVenc
		AND T0."DocCur" = UPPER(:moneda)
		AND T0."CardCode" like '%' || :CardCode || '%'
		AND T0."DocEntry" NOT IN (SELECT "U_SMC_DOCENTRY" FROM "@SMC_APM_ESCDET" 
									WHERE "U_SMC_ESCCAB" = :escenario and "U_SMC_TIPO_DOCUMENTO" = 'NC-C')
		--AND T0."DocTotal" NOT IN (SELECT "U_SMC_MONTO" FROM "@SMC_APM_ESCDET" WHERE "U_SMC_ESCCAB" = :escenario)
		
		
		UNION	
		
		SELECT
		T0."DocEntry",
		T0."DocNum",
		T0."TaxDate" AS "FechaContable",
		T1."DueDate" AS "FechaVencimiento",
		T0."CardCode",
		T0."CardName",
		T0."NumAtCard",
		T0."DocCur",
		CAST( 
		(CASE 
			WHEN T0."DocCur" in ('USD','EUR') 
				THEN T1."InsTotalFC" --- T1."PaidFC" + 
				--(SELECT (R0."InsTotalFC"- R0."PaidFC") FROM PCH6 R0 WHERE R0."DocEntry" = T0."DocEntry" AND R0."InstlmntID" = '1')
			ELSE 
				T1."InsTotal" --- T1."PaidToDate" --+ 
				--(SELECT (R0."InsTotal"- R0."PaidToDate") FROM PCH6 R0 WHERE R0."DocEntry" = T0."DocEntry" AND R0."InstlmntID" = '1')
		END)AS DECIMAL(16,2)) AS "Total",
		
		IFNULL(T5."OffclCode",'') AS "CodigoRetencion" ,
				
		CAST((CASE 
			WHEN T0."DocCur" in ('USD','EUR') THEN 
				T0."WTSumFC"
			ELSE 
				T0."WTSum"
		END)AS DECIMAL(16,2)) AS "Retencion",
		CAST((CASE 	WHEN T0."DocCur" in ('USD','EUR') THEN 
							CASE (SELECT "WTCode" FROM DPO5 WHERE "AbsEntry" = T0."DocEntry")
								WHEN 'RT4C' THEN T1."InsTotalFC" - T1."PaidFC"
								ELSE T1."InsTotalFC" - T1."PaidFC"- T0."WTSumFC"
							END
						ELSE 
							CASE (SELECT "WTCode" FROM DPO5 WHERE "AbsEntry" = T0."DocEntry")
								WHEN 'RT4C' THEN T1."InsTotal" - T1."PaidToDate"
								ELSE T1."InsTotal" - T1."PaidToDate" - T0."WTSum"
							END
			END)AS DECIMAL(16,2)) AS "TotalPagar",
		T3."LicTradNum" AS "RUC",
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."Account") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" /*AND "BankCode" = :tipoBanco*/ AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,'')
		AS "Cuenta",
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" /*AND "BankCode" = :tipoBanco*/ AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,'') 
		AS "CuentaMoneda",
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" /*AND "BankCode" = :tipoBanco*/ AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,'')
		AS "BankCode",
		(CASE 
			WHEN DAYS_BETWEEN(T0."DocDueDate", :fechaVenc) < 0 THEN 0 
			ELSE DAYS_BETWEEN(T0."DocDueDate", :fechaVenc) 
		END) AS "Atraso",
		--0 AS "Atraso",
		'' AS "Marca",
		'' AS "Estado",
		'FA-P' as "Documento",
		T0."Comments" AS "Comentario",
		CASE WHEN T3."QryGroup11" = 'Y' THEN 'PROV. CAJA CHICA' ELSE '' END AS "Propiedad",
		T1."DueDate"
		,ifnull(T0."U_EXC_ORIGEN",'') as "Origen"
		,IFNULL(T0."PayBlock", 'N') AS "BloqueoPago"
		,'N' AS "DetraccionPend"
		,T1."InstlmntID" AS "NroCuota"
		,0 AS "LineaAsiento"
	FROM 
		ODPO T0
		LEFT JOIN DPO6 T1 ON T0."DocEntry" = T1."DocEntry"
		LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
		LEFT JOIN OCRD T3 ON T0."CardCode" = T3."CardCode"
		LEFT JOIN PCH5 T4 ON T0."DocEntry" = T4."AbsEntry" AND T4."ObjType" = T0."ObjType"
		LEFT JOIN OWHT T5 ON T4."WTCode" = T5."WTCode"
		--LEFT JOIN OCRB T4 ON T0."CardCode" = T4."CardCode"
	WHERE 
		T0."Indicator" IN ('00','01','02','08','14','50','99','91','05','07','DI')
		--T0."Indicator" IN ('01','02','03','08','DT','DO')
		AND T1."InstlmntID" = '1'
		AND T1."Status" = 'O'
		AND T2."PymntGroup" not like '%DT%'
		AND T1."DueDate" <= :fechaVenc
		AND T0."DocCur" = UPPER(:moneda)
		AND T0."CardCode" like '%' || :CardCode || '%'
		AND T0."CreateTran" = 'Y'
		AND T0."DocEntry" NOT IN (SELECT "U_SMC_DOCENTRY" FROM "@SMC_APM_ESCDET" 
									WHERE "U_SMC_ESCCAB" = :escenario and "U_SMC_TIPO_DOCUMENTO" = 'FA-P')
		--AND T0."DocTotal" NOT IN (SELECT "U_SMC_MONTO" FROM "@SMC_APM_ESCDET" WHERE "U_SMC_ESCCAB" = :escenario)
		
		
		union
		
		
		SELECT
		T0."DocEntry",
		T0."DocNum",
		T0."TaxDate" AS "FechaContable",
		T1."DueDate" AS "FechaVencimiento",
		T0."CardCode",
		T0."CardName",
		T0."NumAtCard",
		T0."DocCur",
		CAST( 
		(CASE 
			WHEN T0."DocCur" in ('USD','EUR') 
				THEN T1."InsTotalFC" --- T1."PaidFC" + 
				--(SELECT (R0."InsTotalFC"- R0."PaidFC") FROM PCH6 R0 WHERE R0."DocEntry" = T0."DocEntry" AND R0."InstlmntID" = '1')
			ELSE 
				T1."InsTotal" --- T1."PaidToDate" --+ 
				--(SELECT (R0."InsTotal"- R0."PaidToDate") FROM PCH6 R0 WHERE R0."DocEntry" = T0."DocEntry" AND R0."InstlmntID" = '1')
		END)AS DECIMAL(16,2)) AS "Total",
		
		IFNULL(T5."OffclCode",'') AS "CodigoRetencion" ,
				
		CAST((CASE 
			WHEN T0."DocCur" in ('USD','EUR') THEN 
				T0."WTSumFC"
			ELSE 
				T0."WTSum"
		END)AS DECIMAL(16,2)) AS "Retencion",
		CAST((CASE 	WHEN T0."DocCur" in ('USD','EUR') THEN 
							CASE (SELECT "WTCode" FROM DPO5 WHERE "AbsEntry" = T0."DocEntry")
								WHEN 'RT4C' THEN T1."InsTotalFC" - T1."PaidFC"
								ELSE T1."InsTotalFC" - T1."PaidFC"- T0."WTSumFC"
							END
						ELSE 
							CASE (SELECT "WTCode" FROM DPO5 WHERE "AbsEntry" = T0."DocEntry")
								WHEN 'RT4C' THEN T1."InsTotal" - T1."PaidToDate"
								ELSE T1."InsTotal" - T1."PaidToDate" - T0."WTSum"
							END
			END)AS DECIMAL(16,2)) AS "TotalPagar",
		T3."LicTradNum" AS "RUC",
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."Account") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" /*AND "BankCode" = :tipoBanco*/ AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,'')
		AS "Cuenta",
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" /*AND "BankCode" = :tipoBanco*/ AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,'') 
		AS "CuentaMoneda",
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" /*AND "BankCode" = :tipoBanco*/ AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,'')
		AS "BankCode",
		(CASE 
			WHEN DAYS_BETWEEN(T0."DocDueDate", :fechaVenc) < 0 THEN 0 
			ELSE DAYS_BETWEEN(T0."DocDueDate", :fechaVenc) 
		END) AS "Atraso",
		--0 AS "Atraso",
		'' AS "Marca",
		'' AS "Estado",
		'SA-P' as "Documento",
		T0."Comments" AS "Comentario",
		CASE WHEN T3."QryGroup11" = 'Y' THEN 'PROV. CAJA CHICA' ELSE '' END AS "Propiedad",
		T1."DueDate"
		,ifnull(T0."U_EXC_ORIGEN",'') as "Origen"
		,IFNULL(T0."PayBlock", 'N') AS "BloqueoPago"
		,'N' AS "DetraccionPend"
		,T1."InstlmntID" AS "NroCuota"
		,0 AS "LineaAsiento"
	FROM 
		ODPO T0
		LEFT JOIN DPO6 T1 ON T0."DocEntry" = T1."DocEntry"
		LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
		LEFT JOIN OCRD T3 ON T0."CardCode" = T3."CardCode"
		LEFT JOIN PCH5 T4 ON T0."DocEntry" = T4."AbsEntry" AND T4."ObjType" = T0."ObjType"
		LEFT JOIN OWHT T5 ON T4."WTCode" = T5."WTCode"
		--LEFT JOIN OCRB T4 ON T0."CardCode" = T4."CardCode"
	WHERE 
		T0."Indicator" IN ('00','01','02','08','14','50','99','91','05','07','DI')
		--T0."Indicator" IN ('01','02','03','08','DT','DO')
		AND T1."InstlmntID" = '1'
		AND T1."Status" = 'O'
		AND T2."PymntGroup" not like '%DT%'
		AND T1."DueDate" <= :fechaVenc
		AND T0."DocCur" = UPPER(:moneda)
		AND T0."CardCode" like '%' || :CardCode || '%'
		AND T0."CreateTran" = 'N'
		AND T0."DocEntry" NOT IN (SELECT "U_SMC_DOCENTRY" FROM "@SMC_APM_ESCDET" 
									WHERE "U_SMC_ESCCAB" = :escenario and "U_SMC_TIPO_DOCUMENTO" = 'SA-P')
		--AND T0."DocTotal" NOT IN (SELECT "U_SMC_MONTO" FROM "@SMC_APM_ESCDET" WHERE "U_SMC_ESCCAB" = :escenario)
		
		
		union
		
		
		
SELECT
		T0."DocEntry",
		T0."DocNum",
		T0."TaxDate" AS "FechaContable",
		T0."DocDueDate" AS "FechaVencimiento",
		T0."CardCode",
		T0."CardName",
		T0."DocType" as "NumAtCard",
		T0."DocCurr",
		
		CAST( 
		(CASE 
			WHEN T0."DocCurr" in ('USD','EUR') 
				THEN T0."NoDocSumFC" 
			ELSE 
				T0."NoDocSum"
		END)AS DECIMAL(16,2)) AS "Total",
		
		'' AS "CodigoRetencion" ,
				
		0 AS "Retencion",
		CAST( 
		(CASE 
			WHEN T0."DocCurr" in ('USD','EUR') 
				THEN T0."NoDocSumFC" 
			ELSE 
				T0."NoDocSum"
		END)AS DECIMAL(16,2)) AS "TotalPagar",
			
		(select "LicTradNum" from OCRD where "CardCode" = T0."CardCode") AS "RUC",
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."Account") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" /*AND "BankCode" = :tipoBanco*/ AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,'')
		AS "Cuenta",
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" /*AND "BankCode" = :tipoBanco*/ AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,'') 
		AS "CuentaMoneda",
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" /*AND "BankCode" = :tipoBanco*/ AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,'')
		AS "BankCode",
		(CASE 
			WHEN DAYS_BETWEEN(T0."DocDueDate", :fechaVenc) < 0 THEN 0 
			ELSE DAYS_BETWEEN(T0."DocDueDate", :fechaVenc) 
		END) AS "Atraso",
		'' AS "Marca",
		'' AS "Estado",
		'SP' as "Documento",
		T0."Comments" AS "Comentario",
		CASE WHEN T3."QryGroup11" = 'Y' THEN 'PROV. CAJA CHICA' ELSE '' END AS "Propiedad",
		T0."DocDueDate"
		,'' as "Origen"
		,'N' AS "BloqueoPago"
		,'N' AS "DetraccionPend"
		,1 AS "NroCuota"
		,0 AS "LineaAsiento"
	FROM 
		OPDF T0
		LEFT JOIN OCRD T3 ON T0."CardCode" = T3."CardCode"
	WHERE 
		--AND T1."InstlmntID" = '1'

		T0."DocDueDate" <= :fechaVenc
		AND T0."Canceled" = 'N'
		AND T0."DocCurr" = UPPER(:moneda)
		AND T0."CardCode" like '%' || :CardCode || '%'
		AND T0."DocEntry" NOT IN (SELECT "U_SMC_DOCENTRY" FROM "@SMC_APM_ESCDET" 
									WHERE "U_SMC_ESCCAB" = :escenario and "U_SMC_TIPO_DOCUMENTO" = 'SP')
	
		
		UNION	
		
		
		
/*
				
SELECT
		T0."DocEntry",
		T0."DocNum",
		TO_VARCHAR(T0."TaxDate",'DD-MM-YYYY') AS "FechaContable",
		TO_VARCHAR(T0."DocDueDate",'DD-MM-YYYY') AS "FechaVencimiento",
		T0."CardCode",
		T0."CardName",
		T0."DocType" as "NumAtCard",
		T0."DocCurr",
		
		CAST( 
		(CASE 
			WHEN T0."DocCurr" in ('USD','EUR') 
				THEN T0."NoDocSumFC" 
			ELSE 
				T0."NoDocSum"
		END)AS DECIMAL(16,2)) AS "Total",
		0 AS "Retencion",
		CAST( 
		(CASE 
			WHEN T0."DocCurr" in ('USD','EUR') 
				THEN T0."NoDocSumFC" 
			ELSE 
				T0."NoDocSum"
		END)AS DECIMAL(16,2)) AS "TotalPagar",
			
		(select "LicTradNum" from OCRD where "CardCode" = T0."CardCode") AS "RUC",
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."Account") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = :tipoBanco AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,'')
		AS "Cuenta",
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = :tipoBanco AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,'') 
		AS "CuentaMoneda",
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = :tipoBanco AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",:moneda) = :moneda)and "U_EXC_ACTIVO" = 'Y'))
		,'')
		AS "BankCode",
		(CASE 
			WHEN DAYS_BETWEEN(T0."DocDueDate", :fechaVenc) < 0 THEN 0 
			ELSE DAYS_BETWEEN(T0."DocDueDate", :fechaVenc) 
		END) AS "Atraso",
		'' AS "Marca",
		'' AS "Estado",
		'PR' as "Documento",
		T0."Comments" AS "Comentario",
		CASE WHEN T3."QryGroup11" = 'Y' THEN 'PROV. CAJA CHICA' ELSE '' END AS "Propiedad",
		T0."DocDueDate"
		,'' as "Origen"
	FROM 
		ORCT T0
		INNER JOIN OCRD T3 ON T0."CardCode" = T3."CardCode"
	WHERE 
		--AND T1."InstlmntID" = '1'

		T0."DocDueDate" <= :fechaVenc
		AND T0."Canceled" = 'N'
		AND T0."DocCurr" = UPPER(:moneda)
		AND T0."CardCode" like '%' || :CardCode || '%'
		AND T0."DocEntry" NOT IN (SELECT "U_SMC_DOCENTRY" FROM "@SMC_APM_ESCDET" 
									WHERE "U_SMC_ESCCAB" = :escenario and "U_SMC_TIPO_DOCUMENTO" = 'PR')
		AND T0."PayNoDoc"='Y'
		
		
*/
		
	


	--------pagos recibicos-------

SELECT
DISTINCT
			T0."TransId" as "DocEntry",
		T0."Number" as "DocNum",
		T0."TaxDate" AS "FechaContable",
		T1."DueDate" AS "FechaVencimiento",
		T3."CardCode",
		T3."CardName",
		'' as "NumAtCard",
		--T0."TransCurr"
		ifnull(T1."FCCurrency",'SOL')
		,
		CAST( 
		(CASE 
			WHEN ifnull(T1."FCCurrency",'SOL') in ('USD','EUR') 
			
				THEN (T1."FCCredit" - ifnull((select sum(TT0."ReconSumFC") from 
		"ITR1" TT0 where TT0."TransId" = T0."TransId" AND TT0."TransRowId"= T1."Line_ID"
		GROUP BY TT0."TransId",TT0."TransRowId"),2)) 
		
			ELSE 
			
				(T1."Credit" - ifnull((select sum(TT0."ReconSum") from 
		"ITR1" TT0 where TT0."TransId" = T0."TransId" AND TT0."TransRowId"= T1."Line_ID"
		GROUP BY TT0."TransId",TT0."TransRowId"),0))
		
		END)AS DECIMAL(16,2)) AS "Total",
		
		'' AS "CodigoRetencion" ,
		0 AS "Retencion",
		CAST( 
		(CASE 
			WHEN ifnull(T1."FCCurrency",'SOL') in ('USD','EUR') 
			
				THEN (T1."FCCredit" - ifnull((select sum(TT0."ReconSumFC") from 
		"ITR1" TT0 where TT0."TransId" = T0."TransId" AND TT0."TransRowId"= T1."Line_ID"
		GROUP BY TT0."TransId",TT0."TransRowId"),0)) 
		
			ELSE 
			
				(T1."Credit" - ifnull((select sum(TT0."ReconSum") from 
		"ITR1" TT0 where TT0."TransId" = T0."TransId" AND TT0."TransRowId"= T1."Line_ID"
		GROUP BY TT0."TransId",TT0."TransRowId"),0))
		
		END)AS DECIMAL(16,2)) AS "TotalPagar",
			
		T3."LicTradNum" AS "RUC",
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."Account") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" /*AND "BankCode" = :tipoBanco*/ AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,'')
		AS "Cuenta",
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" /*AND "BankCode" = :tipoBanco*/ AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,'') 
		AS "CuentaMoneda",
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" /*AND "BankCode" = :tipoBanco*/ AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,'')
		AS "BankCode",
		(CASE 
			WHEN DAYS_BETWEEN(T0."DueDate", :fechaVenc) < 0 THEN 0 
			ELSE DAYS_BETWEEN(T0."DueDate", :fechaVenc) 
		END) AS "Atraso",
		--0 AS "Atraso",
		'' AS "Marca",
		'' AS "Estado",
		'PR' as "Documento",
		'' AS "Comentario",
		CASE WHEN T3."QryGroup11" = 'Y' THEN 'PROV. CAJA CHICA' ELSE '' END AS "Propiedad",
		T0."DueDate"
		,'' as "Origen"
		,'N' AS "BloqueoPago"
		,'N' AS "DetraccionPend"
		,0 AS "NroCuota"
		,T1."Line_ID" AS "LineaAsiento"
	FROM 
		OJDT T0
		LEFT JOIN JDT1 T1 ON T0."TransId" = T1."TransId"
		--LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
		inner JOIN OCRD T3 ON T1."ShortName" = T3."CardCode"
		--LEFT JOIN OCRB T4 ON T0."CardCode" = T4."CardCode"
	WHERE 
	 	T0."TransType" = 24

		/*
		AND ifnull((select count(*) from "ITR1" TT0 where TT0."TransId" = T0."TransId" GROUP BY TT0."TransId"),0) = 0
		*/
		and (T1."Credit" - 
		ifnull((select sum(TT0."ReconSum") from 
		"ITR1" TT0 where TT0."TransId" = T0."TransId" AND TT0."TransRowId"= T1."Line_ID"
		GROUP BY TT0."TransId",TT0."TransRowId"),0)) > 0
		

		AND T1."DueDate" <= :fechaVenc
		AND ifnull(T1."FCCurrency",'SOL') = UPPER(:moneda)
		AND T3."CardCode" like '%' || :CardCode || '%'
		and T1."Credit">0
		AND T0."TransId" NOT IN (SELECT "U_SMC_DOCENTRY" FROM "@SMC_APM_ESCDET" 
									WHERE "U_SMC_ESCCAB" = :escenario and "U_SMC_TIPO_DOCUMENTO" = 'PR')		
		
		



-------pagos recibidos-------


		UNION
		
		
		---------AS-------------------------
		SELECT
DISTINCT
		T0."TransId" as "DocEntry",
		T0."Number" as "DocNum",
		T0."TaxDate" AS "FechaContable",
		T1."DueDate" AS "FechaVencimiento",
		T3."CardCode",
		T3."CardName",
		'' as "NumAtCard",
		--T0."TransCurr"
		ifnull(T1."FCCurrency",'SOL')
		,
		CAST( 
		(CASE 
			WHEN ifnull(T1."FCCurrency",'SOL') in ('USD','EUR') 
			
				THEN (T1."FCCredit" - ifnull((select sum(TT0."ReconSumFC") from 
		"ITR1" TT0 where TT0."TransId" = T0."TransId"  AND TT0."TransRowId" = T1."Line_ID"
		GROUP BY TT0."TransId",TT0."TransRowId"),0)) 
		
			ELSE 
			
				(T1."Credit" - ifnull((select sum(TT0."ReconSum") from 
		"ITR1" TT0 where TT0."TransId" = T0."TransId"  AND TT0."TransRowId" = T1."Line_ID"
		GROUP BY TT0."TransId",TT0."TransRowId"),0))
				
		END)AS DECIMAL(16,2)) AS "Total",
		
		T2."WTCode" AS "CodigoRetencion" ,
		
		CAST( 
		(CASE 
			WHEN ifnull(T1."FCCurrency",'SOL') in ('USD','EUR') 
				THEN (T2."WTAmntFC") 
			ELSE 
				(T2."WTAmnt")
				
		END)AS DECIMAL(16,2)) AS "Retencion",
		
		CAST( 
		(CASE 
			WHEN ifnull(T1."FCCurrency",'SOL') in ('USD','EUR') 
				THEN (T1."FCCredit" - ifnull((select sum(TT0."ReconSumFC") from 
		"ITR1" TT0 where TT0."TransId" = T0."TransId"  AND TT0."TransRowId" = T1."Line_ID"
		GROUP BY TT0."TransId",TT0."TransRowId"),0)) 
			ELSE 
				(T1."Credit" - ifnull((select sum(TT0."ReconSum") from 
		"ITR1" TT0 where TT0."TransId" = T0."TransId"  AND TT0."TransRowId" = T1."Line_ID"
		GROUP BY TT0."TransId",TT0."TransRowId"),0))
				
		END)AS DECIMAL(16,2)) AS "TotalPagar",
			
		T3."LicTradNum" AS "RUC",
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."Account") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" /*AND "BankCode" = :tipoBanco*/ AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,'')
		AS "Cuenta",
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" /*AND "BankCode" = :tipoBanco*/ AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,'') 
		AS "CuentaMoneda",
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" /*AND "BankCode" = :tipoBanco*/ AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",'SOL') = 'SOL')and "U_EXC_ACTIVO" = 'Y'))
		,'')
		AS "BankCode",
		(CASE 
			WHEN DAYS_BETWEEN(T0."DueDate", :fechaVenc) < 0 THEN 0 
			ELSE DAYS_BETWEEN(T0."DueDate", :fechaVenc) 
		END) AS "Atraso",
		--0 AS "Atraso",
		'' AS "Marca",
		'' AS "Estado",
		'AS' as "Documento",
		'' AS "Comentario",
		CASE WHEN T3."QryGroup11" = 'Y' THEN 'PROV. CAJA CHICA' ELSE '' END AS "Propiedad",
		T0."DueDate"
		,'' as "Origen"
		,'N' AS "BloqueoPago"
		,'N' AS "DetraccionPend"
		,0 AS "NroCuota"
		,T1."Line_ID" AS "LineaAsiento"
	FROM 
		OJDT T0
		LEFT JOIN JDT1 T1 ON T0."TransId" = T1."TransId"
		LEFT JOIN JDT2 T2 ON T1."TransId" = T2."AbsEntry"
		--LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
		inner JOIN OCRD T3 ON T1."ShortName" = T3."CardCode"
		--LEFT JOIN OCRB T4 ON T0."CardCode" = T4."CardCode"
	WHERE 
	 	T0."TransType" = 30
	
		/*
		AND ifnull((select count(*) from "ITR1" TT0 where TT0."TransId" = T0."TransId" GROUP BY TT0."TransId"),0) = 0
		*/
			and (T1."Credit" - 
		ifnull((select sum(TT0."ReconSum") from 
		"ITR1" TT0 where TT0."TransId" = T0."TransId" AND TT0."TransRowId" = T1."Line_ID"
		GROUP BY TT0."TransId", TT0."TransRowId"),0)) > 0
		
		AND T1."DueDate" <= :fechaVenc
		AND ifnull(T1."FCCurrency",'SOL') = UPPER(:moneda)
		AND T3."CardCode" like '%' || :CardCode || '%'
		and T1."Credit">0
		/*AND T0."TransId" NOT IN (SELECT "U_SMC_DOCENTRY" FROM "@SMC_APM_ESCDET" 
									WHERE "U_SMC_ESCCAB" = :escenario and "U_SMC_TIPO_DOCUMENTO" = 'AS')*/
		AND ifnull((SELECT max('Y') FROM "@SMC_APM_ESCDET" 
									WHERE "U_SMC_ESCCAB" = :escenario 
		and "U_SMC_TIPO_DOCUMENTO" = 'AS' and "U_SMC_DOCENTRY" = T0."TransId" and "U_EXP_LINEAASIENTO" = T1."Line_ID"),'N') != 'Y'		
		
		) T0
	WHERE
		T0."Total" > 0 
		--and T0."BankCode" like '%'||:filtroBanco||'%' --se agrego nuevo

	ORDER BY 
		T0."FechaVencimiento" desc;  
		
END;