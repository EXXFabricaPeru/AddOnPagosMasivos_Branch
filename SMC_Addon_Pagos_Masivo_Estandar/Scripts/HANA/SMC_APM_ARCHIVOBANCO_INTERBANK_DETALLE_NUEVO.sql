CREATE PROCEDURE "SMC_APM_ARCHIVOBANCO_INTERBANK_DETALLE_NUEVO"
	(escenario varchar(15))
AS
BEGIN	 

	lt_data_tmp_1 =(
		SELECT 
			Z0."Proveedor",
			SUM(Z0."Montototal") AS "MontototalSum"
		FROM
		(SELECT
			T3."CardCode" AS "Proveedor",
			CAST(IFNULL(T4."U_EXP_IMPORTE",(CASE 
				WHEN T0."DocCur" in ('USD','EUR') 
					THEN T1."InsTotalFC" - T1."PaidFC"
					- T0."WTSumFC"
				ELSE 
					T1."InsTotal" - T1."PaidToDate" 
					- T0."WTSum"
			END))AS DECIMAL(16,2))
			AS "Montototal"
		FROM 
			OPCH T0
			LEFT JOIN PCH6 T1 ON T0."DocEntry" = T1."DocEntry"
			LEFT JOIN OCRD T3 ON T0."CardCode" = T3."CardCode"
			LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
			LEFT JOIN "@EXP_PMP1" T4 
			ON T0."DocEntry" = T4."U_EXP_DOCENTRYDOC" 
			AND T1."DocEntry" = T4."U_EXP_DOCENTRYDOC" AND T4."U_EXP_TIPODOC" = T0."ObjType"
		WHERE
			T0."Indicator" IN ('00','01','02','14','50','99','05')
			AND T1."InstlmntID" = '1'
			AND T2."PymntGroup" like '%DT%'
			AND IFNULL(T4."U_EXP_SLC_PAGO",'N') = 'Y'
			AND IFNULL(T4."U_EXP_SLC_RETENCION",'N') = 'N'
			AND T4."DocEntry" = :escenario
			--AND T4."U_SMC_TIPO_DOCUMENTO" = 'FT-P'
			AND T0."DocStatus" != 'C'
			
		UNION
		
		SELECT
			T3."CardCode" AS "Proveedor",
			CAST(IFNULL(T4."U_EXP_IMPORTE",(CASE 
				WHEN T0."DocCur" in ('USD','EUR') 
					THEN T1."InsTotalFC" - T1."PaidFC"
					- T0."WTSumFC"
				ELSE 
					T1."InsTotal" - T1."PaidToDate" 
					- T0."WTSum"
			END))AS DECIMAL(16,2))
			AS "Montototal"
		FROM 
			OPCH T0
			LEFT JOIN PCH6 T1 ON T0."DocEntry" = T1."DocEntry" AND T1."InstlmntID" = '1'
			LEFT JOIN OCRD T3 ON T0."CardCode" = T3."CardCode"
			LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
			LEFT JOIN "@EXP_PMP1" T4 
			ON T0."DocEntry" = T4."U_EXP_DOCENTRYDOC" 
			AND T1."DocEntry" = T4."U_EXP_DOCENTRYDOC" AND T4."U_EXP_TIPODOC" = T0."ObjType"
			--AND T4."U_SMC_TIPO_DOCUMENTO" = 'FT-P'
		WHERE
			T0."Indicator" IN ('00','01','02','14','50','99','05')
			AND T1."InstlmntID" = '1'
			AND T2."PymntGroup" not like '%DT%'
			AND IFNULL(T4."U_EXP_SLC_PAGO",'N') = 'Y'
			AND IFNULL(T4."U_EXP_SLC_RETENCION",'N') = 'N'
			AND T4."DocEntry" = :escenario
			--AND T4."U_SMC_TIPO_DOCUMENTO" = 'FT-P'
			AND T0."DocStatus" != 'C'
			
			union
			
			
			--nuevo


			SELECT
			T3."CardCode" AS "Proveedor",
			CAST(IFNULL(T4.U_SMC_MONTO,(CASE 
				WHEN T0."DocCur" in ('USD','EUR') 
					THEN T1."InsTotalFC" - T1."PaidFC"
					- T0."WTSumFC"
				ELSE 
					T1."InsTotal" - T1."PaidToDate" 
					- T0."WTSum"
			END))AS DECIMAL(16,2))
			AS "Montototal"
		FROM 
			ORIN T0
			LEFT JOIN RIN6 T1 ON T0."DocEntry" = T1."DocEntry" AND T1."InstlmntID" = '1'
			LEFT JOIN OCRD T3 ON T0."CardCode" = T3."CardCode"
			LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
			LEFT JOIN "@SMC_APM_ESCDET" T4 
			ON T0."DocEntry" = T4."U_SMC_DOCENTRY" 
			AND T1."DocEntry" = T4."U_SMC_DOCENTRY"
			AND T4."U_SMC_TIPO_DOCUMENTO" = 'NC-C'
		WHERE
			T0."Indicator" IN ('00','01','02','14','50','99','05','07')
			AND T1."InstlmntID" = '1'
			AND T2."PymntGroup" not like '%DT%'
			AND IFNULL(T4."U_SMC_PAGO",'N') = 'Y'
			AND IFNULL(T4."U_SMC_REG",'N') = 'N'
			AND T4."U_SMC_ESCCAB" = :escenario
			AND T4."U_SMC_TIPO_DOCUMENTO" = 'NC-C'
			AND T0."DocStatus" != 'C'
			
			union
			
			
			SELECT
			T3."CardCode" AS "Proveedor",
			CAST(IFNULL(T4.U_SMC_MONTO,(CASE 
				WHEN T0."DocCur" in ('USD','EUR') 
					THEN T1."InsTotalFC" - T1."PaidFC"
					- T0."WTSumFC"
				ELSE 
					T1."InsTotal" - T1."PaidToDate" 
					- T0."WTSum"
			END))AS DECIMAL(16,2))
			AS "Montototal"
		FROM 
			ODPO T0
			LEFT JOIN DPO6 T1 ON T0."DocEntry" = T1."DocEntry" AND T1."InstlmntID" = '1'
			LEFT JOIN OCRD T3 ON T0."CardCode" = T3."CardCode"
			LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
			LEFT JOIN "@SMC_APM_ESCDET" T4 
			ON T0."DocEntry" = T4."U_SMC_DOCENTRY" 
			AND T1."DocEntry" = T4."U_SMC_DOCENTRY"
			AND T4."U_SMC_TIPO_DOCUMENTO" = 'FA-P'
		WHERE
			T0."Indicator" IN ('00','01','02','14','50','99','05','07','DI')
			AND T1."InstlmntID" = '1'
			AND T0."CreateTran" = 'Y'
			AND T2."PymntGroup" not like '%DT%'
			AND IFNULL(T4."U_SMC_PAGO",'N') = 'Y'
			AND IFNULL(T4."U_SMC_REG",'N') = 'N'
			AND T4."U_SMC_ESCCAB" = :escenario
			AND T4."U_SMC_TIPO_DOCUMENTO" = 'FA-P'
			AND T0."DocStatus" != 'C'
			
			union
			
			SELECT
			T3."CardCode" AS "Proveedor",
			CAST(IFNULL(T4.U_SMC_MONTO,(CASE 
				WHEN T0."DocCur" in ('USD','EUR') 
					THEN T1."InsTotalFC" - T1."PaidFC"
					- T0."WTSumFC"
				ELSE 
					T1."InsTotal" - T1."PaidToDate" 
					- T0."WTSum"
			END))AS DECIMAL(16,2))
			AS "Montototal"
		FROM 
			ODPO T0
			LEFT JOIN DPO6 T1 ON T0."DocEntry" = T1."DocEntry" AND T1."InstlmntID" = '1'
			LEFT JOIN OCRD T3 ON T0."CardCode" = T3."CardCode"
			LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
			LEFT JOIN "@SMC_APM_ESCDET" T4 
			ON T0."DocEntry" = T4."U_SMC_DOCENTRY" 
			AND T1."DocEntry" = T4."U_SMC_DOCENTRY"
			AND T4."U_SMC_TIPO_DOCUMENTO" = 'SA-P'
		WHERE
			T0."Indicator" IN ('00','01','02','14','50','99','05','07','DI')
			AND T1."InstlmntID" = '1'
			AND T0."CreateTran" = 'N'
			AND T2."PymntGroup" not like '%DT%'
			AND IFNULL(T4."U_SMC_PAGO",'N') = 'Y'
			AND IFNULL(T4."U_SMC_REG",'N') = 'N'
			AND T4."U_SMC_ESCCAB" = :escenario
			AND T4."U_SMC_TIPO_DOCUMENTO" = 'SA-P'
			AND T0."DocStatus" != 'C'
			
			
			
			union
			
			
			SELECT
			T3."CardCode" AS "Proveedor",
			
			CAST( 
			(CASE 
				WHEN T0."DocCurr" in ('USD','EUR') 
					THEN T0."NoDocSumFC" 
				ELSE 
					T0."NoDocSum"
			END)AS DECIMAL(16,2)) AS "Montototal"
			
		FROM 
			OPDF T0
			--LEFT JOIN DPO6 T1 ON T0."DocEntry" = T1."DocEntry" AND T1."InstlmntID" = '1'
			LEFT JOIN OCRD T3 ON T0."CardCode" = T3."CardCode"
			--LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
			LEFT JOIN "@SMC_APM_ESCDET" T4 
			ON T0."DocEntry" = T4."U_SMC_DOCENTRY"
			--AND T1."DocEntry" = T4."U_SMC_DOCENTRY"
			AND T4."U_SMC_TIPO_DOCUMENTO" = 'SP'
		WHERE
			--T0."Indicator" IN ('01','02','14','50','99','05','07','DI')
			--AND T1."InstlmntID" = '1'
			--AND T0."CreateTran" = 'N'
			--AND T2."PymntGroup" not like '%DT%'
			IFNULL(T4."U_SMC_PAGO",'N') = 'Y'
			AND IFNULL(T4."U_SMC_REG",'N') = 'N'
			AND T4."U_SMC_ESCCAB" = :escenario
			AND T4."U_SMC_TIPO_DOCUMENTO" = 'SP'
			
			
			
			
			union
			
			
			SELECT
			T3."CardCode" AS "Proveedor",
			
			CAST( 
			(CASE 
				WHEN ifnull(T0."TransCurr",'SOL') in ('USD','EUR') 
					THEN (T0."FcTotal" - (select sum(TT0."ReconSumFC") from 
					"ITR1" TT0 where TT0."TransId" = T0."TransId"
					GROUP BY TT0."TransId")) 
				ELSE 
					(T0."LocTotal" - (select sum(TT0."ReconSum") from 
					"ITR1" TT0 where TT0."TransId" = T0."TransId"
					GROUP BY TT0."TransId"))
			END)AS DECIMAL(16,2)) AS "Montototal"
			
		FROM 
			OJDT T0
			--LEFT JOIN DPO6 T1 ON T0."DocEntry" = T1."DocEntry" AND T1."InstlmntID" = '1'
			LEFT JOIN JDT1 T1 ON T0."TransId" = T1."TransId"
			inner JOIN OCRD T3 ON T1."ShortName" = T3."CardCode"
			--LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
			LEFT JOIN "@SMC_APM_ESCDET" T4 
			ON T0."TransId" = T4."U_SMC_DOCENTRY"
			--AND T1."DocEntry" = T4."U_SMC_DOCENTRY"
			AND T4."U_SMC_TIPO_DOCUMENTO" = 'AS'
		WHERE
			--T0."Indicator" IN ('01','02','14','50','99','05','07','DI')
			--AND T1."InstlmntID" = '1'
			--AND T0."CreateTran" = 'N'
			--AND T2."PymntGroup" not like '%DT%'
			IFNULL(T4."U_SMC_PAGO",'N') = 'Y'
			AND IFNULL(T4."U_SMC_REG",'N') = 'N'
			AND T4."U_SMC_ESCCAB" = :escenario
			and T1."Credit" > 0
			AND T4."U_SMC_TIPO_DOCUMENTO" = 'AS'
			
			
			union
			
			
			SELECT
			T3."CardCode" AS "Proveedor",
			
			CAST( 
			(CASE 
				WHEN ifnull(T1."FCCurrency",'SOL') in ('USD','EUR') 
					THEN 
					(T0."FcTotal" - (select sum(TT0."ReconSumFC") from 
					"ITR1" TT0 where TT0."TransId" = T0."TransId"
					GROUP BY TT0."TransId")) 
				ELSE 
					(T0."LocTotal" - (select sum(TT0."ReconSum") from 
					"ITR1" TT0 where TT0."TransId" = T0."TransId"
					GROUP BY TT0."TransId"))
				
			END)AS DECIMAL(16,2)) AS "Montototal"
			
		FROM 
			OJDT T0
			--LEFT JOIN DPO6 T1 ON T0."DocEntry" = T1."DocEntry" AND T1."InstlmntID" = '1'
			LEFT JOIN JDT1 T1 ON T0."TransId" = T1."TransId"
			inner JOIN OCRD T3 ON T1."ShortName" = T3."CardCode"
			--LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
			LEFT JOIN "@SMC_APM_ESCDET" T4 
			ON T0."TransId" = T4."U_SMC_DOCENTRY"
			--AND T1."DocEntry" = T4."U_SMC_DOCENTRY"
			AND T4."U_SMC_TIPO_DOCUMENTO" = 'PR'
		WHERE
			--T0."Indicator" IN ('01','02','14','50','99','05','07','DI')
			--AND T1."InstlmntID" = '1'
			--AND T0."CreateTran" = 'N'
			--AND T2."PymntGroup" not like '%DT%'
			IFNULL(T4."U_SMC_PAGO",'N') = 'Y'
			AND IFNULL(T4."U_SMC_REG",'N') = 'N'
			AND T4."U_SMC_ESCCAB" = :escenario
			and T1."Credit" > 0
			AND T4."U_SMC_TIPO_DOCUMENTO" = 'PR'
			
			
			
			
			
			) Z0
		GROUP BY
		Z0."Proveedor"
		);
	
	CREATE LOCAL TEMPORARY TABLE  "#tmpTotalDetSum" AS 
	(
		select * from :lt_data_tmp_1
	);
	
	select * from(
	SELECT
		'2' AS "TipoRegistro",
		
		/*
	 	CASE(IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))  
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '004' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,''))
			WHEN '003' THEN 
				CASE LENGTH(
					TRIM(IFNULL(
					(SELECT MAX(R0."Account") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur")
					,'')
					))
					WHEN 14 THEN 'C'
					ELSE 'I'
				END
			ELSE 'I'
		END
		*/
		
		(SELECT (CASE WHEN MAX(R0."Account") is null then 'I' else 'C' end) FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode"
		AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y')
		AS "TipoCuenta",
		
		RPAD((IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		(SELECT MAX(R0."Account") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '004' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,'9')),20)
		AS "CuentaAbono",
		
		'1' AS "Caracter",
		(CASE WHEN T3."U_EXX_TIPODOCU" = '6' then '02'
			WHEN T3."U_EXX_TIPODOCU" = '1'then '01' END) AS "TipoDocumentoIdentidad",
		--T3."U_EXX_TIPODOCU" AS "TipoDocumentoIdentidad",

		RPAD(T3."LicTradNum",15) AS "NumeroDocumentoIdentidad",
		RPAD(T3."LicTradNum",20) AS "NumeroDocumentoIdentidad1",
		
		RPAD(T3."CardName",60) AS "NombreProveedor",
		RPAD('Referencia Beneficiario ' || T3."LicTradNum",40) AS "ReferenciaBeneficiario",
		RPAD('Ref Emp ' || T3."LicTradNum",20) AS "Referencia",
		CASE
			WHEN T0."DocCur" = 'SOL' THEN '01'
			ELSE '10' 
		END AS "Moneda",
		LPAD(CAST(
			CAST(IFNULL(T4."U_EXP_IMPORTE",(CASE 
				WHEN T0."DocCur" in ('USD','EUR') 
					THEN T1."InsTotalFC" - T1."PaidFC"
					- T0."WTSumFC"
				ELSE 
					T1."InsTotal" - T1."PaidToDate" 
					- T0."WTSum"
			END))AS DECIMAL(16,2))
		AS VARCHAR),16,'0')
		AS "ImporteParcial",
		LPAD(CAST(CAST(
			T6."MontototalSum"
		AS DECIMAL(16,2))AS VARCHAR),16,'0')
		AS "Importetotal",
		'S' AS "ValidacionIDC",
		'F' AS "TipoDocumentoPagar",
		/*
		LPAD(SUBSTRING(T0."NumAtCard",0,LOCATE(T0."NumAtCard",'-')-1) || 
		LPAD(SUBSTRING(T0."NumAtCard",LOCATE(T0."NumAtCard",'-')+1,LENGTH(T0."NumAtCard")-LOCATE(T0."NumAtCard",'-')+1),7,'0')
		,15,'0')
		*/
		
		LPAD(IFNULL(REPLACE(T0."NumAtCard",'-',''),'0'),10,'0')
		AS "NumeroDocumento"
		
		,T0."FolioPref" as "SerieDocumento"
		,TO_VARCHAR(T0."DocDueDate",'YYYYMMDD') as "FechaVencimiento"
		,(CASE WHEN T3."U_EXX_TIPOPERS" = 'TPJ' THEN 'C'
				WHEN T3."U_EXX_TIPOPERS" = 'TPN' THEN 'P' END) as "TipoPersona"
	
		
	FROM 
		OPCH T0
		LEFT JOIN PCH6 T1 ON T0."DocEntry" = T1."DocEntry"
		LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
		LEFT JOIN OCRD T3 ON T0."CardCode" = T3."CardCode"
		LEFT JOIN "@EXP_PMP1" T4 
		ON T0."DocEntry" = T4."U_EXP_DOCENTRYDOC" 
		AND T1."DocEntry" = T4."U_EXP_DOCENTRYDOC" AND T4."U_EXP_TIPODOC" = T0."ObjType"
		LEFT JOIN "@EXP_OPMP" T5 ON T5."DocEntry" = T4."DocEntry"
		LEFT JOIN "#tmpTotalDetSum" T6 ON T3."CardCode" = T6."Proveedor"
	WHERE 
		T0."Indicator" IN ('00','01','02','14','50','99','05')
		AND T1."InstlmntID" = '2'
		AND T2."PymntGroup" like '%DT%'
		AND IFNULL(T4."U_EXP_SLC_PAGO",'N') = 'Y'
		AND IFNULL(T4."U_EXP_SLC_RETENCION",'N') = 'N'
		AND T5."DocEntry" = :escenario
		--AND T4."U_SMC_TIPO_DOCUMENTO" = 'FT-P'
		--AND T0."DocTotal" = T4."U_SMC_MONTO"
		AND T0."DocStatus" != 'C'
		
		
	UNION

	SELECT
		'2' AS "TipoRegistro",
		/*
	 	CASE(IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '004' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,''))
			WHEN '003' THEN 
				CASE LENGTH(
					TRIM(IFNULL(
					(SELECT MAX(R0."Account") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur")
					,'')
					))
					WHEN 14 THEN 'C'
					ELSE 'I'
				END
			ELSE 'I'
		END*/
		(SELECT (CASE WHEN MAX(R0."Account") is null then 'I' else 'C' end) FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode"
		AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y')
		AS "TipoCuenta",
		
		RPAD((IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		(SELECT MAX(R0."Account") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '004' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,'9')),20)
		AS "CuentaAbono",
		'1' AS "Caracter",
		(CASE WHEN T3."U_EXX_TIPODOCU" = '6' then '02'
			WHEN T3."U_EXX_TIPODOCU" = '1'then '01' END) AS "TipoDocumentoIdentidad",
		--T3."U_EXX_TIPODOCU" AS "TipoDocumentoIdentidad",
		RPAD(T3."LicTradNum",15) AS "NumeroDocumentoIdentidad",
		RPAD(T3."LicTradNum",20) AS "NumeroDocumentoIdentidad1",
		
		RPAD(T3."CardName",60) AS "NombreProveedor",
		RPAD('Referencia Beneficiario ' || T3."LicTradNum",40) AS "ReferenciaBeneficiario",
		RPAD('Ref Emp ' || T3."LicTradNum",20) AS "Referencia",
		CASE
			WHEN T0."DocCur" = 'SOL' THEN '01'
			ELSE '10' 
		END AS "Moneda",
		LPAD(CAST(
			CAST(IFNULL(T4."U_EXP_IMPORTE",(CASE 
				WHEN T0."DocCur" in ('USD','EUR') 
					THEN T1."InsTotalFC" - T1."PaidFC"
					- T0."WTSumFC"
				ELSE 
					T1."InsTotal" - T1."PaidToDate" 
					- T0."WTSum"
			END))AS DECIMAL(16,2))
		AS VARCHAR),16,'0')
		AS "ImporteParcial",
		LPAD(CAST(CAST(
			T6."MontototalSum"
		AS DECIMAL(16,2))AS VARCHAR),16,'0')
		AS "Importetotal",
		'S' AS "ValidacionIDC",
		'F' AS "TipoDocumentoPagar",
		/*
		LPAD(SUBSTRING(T0."NumAtCard",0,LOCATE(T0."NumAtCard",'-')-1) || 
		LPAD(SUBSTRING(T0."NumAtCard",LOCATE(T0."NumAtCard",'-')+1,LENGTH(T0."NumAtCard")-LOCATE(T0."NumAtCard",'-')+1),7,'0')
		,15,'0')
		--LPAD(REPLACE(T0."NumAtCard",'-',''),15,'0') */
		LPAD(IFNULL(REPLACE(T0."NumAtCard",'-',''),'0'),10,'0')
		AS "NumeroDocumento"
		,T0."FolioPref" as "SerieDocumento"
		,TO_VARCHAR(T0."DocDueDate",'YYYYMMDD') as "FechaVencimiento"
		,(CASE WHEN T3."U_EXX_TIPOPERS" = 'TPJ' THEN 'C'
				WHEN T3."U_EXX_TIPOPERS" = 'TPN' THEN 'P' END) as "TipoPersona"
		
	FROM 
		OPCH T0
		LEFT JOIN PCH6 T1 ON T0."DocEntry" = T1."DocEntry" AND T1."InstlmntID" = '1'
		LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
		LEFT JOIN OCRD T3 ON T0."CardCode" = T3."CardCode"
		LEFT JOIN "@EXP_PMP1" T4 
		ON T0."DocEntry" = T4."U_EXP_DOCENTRYDOC" 
		AND T1."DocEntry" = T4."U_EXP_DOCENTRYDOC" AND T4."U_EXP_TIPODOC" = T0."ObjType"
	    LEFT JOIN "@EXP_OPMP" T5 ON T5."DocEntry" = T4."DocEntry" 
	    LEFT JOIN "#tmpTotalDetSum" T6 ON T3."CardCode" = T6."Proveedor"
	WHERE 
		T0."Indicator" IN ('00','01','02','14','50','99','05')
		AND T1."InstlmntID" = '1'
		AND T2."PymntGroup" not like '%DT%'
		AND IFNULL(T4."U_EXP_SLC_PAGO",'N') = 'Y'
		AND IFNULL(T4."U_EXP_SLC_RETENCION",'N') = 'N'
		AND T5."DocEntry" = :escenario
		--AND T4."U_SMC_TIPO_DOCUMENTO" = 'FT-P'
		--AND T0."DocTotal" = T4."U_SMC_MONTO"
		AND T0."DocStatus" != 'C'
	
		
		--nuevoo
	UNION
	
	
	SELECT
		'2' AS "TipoRegistro",
		/*
	 	CASE(IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '004' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,''))
			WHEN '003' THEN 
				CASE LENGTH(
					TRIM(IFNULL(
					(SELECT MAX(R0."Account") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur")
					,'')
					))
					WHEN 14 THEN 'C'
					ELSE 'I'
				END
			ELSE 'I'
		END*/
		(SELECT (CASE WHEN MAX(R0."Account") is null then 'I' else 'C' end) FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode"
		AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y')
		AS "TipoCuenta",
		RPAD((IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		(SELECT MAX(R0."Account") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '004' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,'9')),20)
		AS "CuentaAbono",
		'1' AS "Caracter",
		(CASE WHEN T3."U_EXX_TIPODOCU" = '6' then '02'
			WHEN T3."U_EXX_TIPODOCU" = '1'then '01' END) AS "TipoDocumentoIdentidad",
		--T3."U_EXX_TIPODOCU" AS "TipoDocumentoIdentidad",
		RPAD(T3."LicTradNum",15) AS "NumeroDocumentoIdentidad",
		RPAD(T3."LicTradNum",20) AS "NumeroDocumentoIdentidad1",
		
		RPAD(T3."CardName",60) AS "NombreProveedor",
		RPAD('Referencia Beneficiario ' || T3."LicTradNum",40) AS "ReferenciaBeneficiario",
		RPAD('Ref Emp ' || T3."LicTradNum",20) AS "Referencia",
		CASE
			WHEN T0."DocCur" = 'SOL' THEN '01'
			ELSE '10' 
		END AS "Moneda",
		LPAD(CAST(
			CAST(IFNULL(T4.U_SMC_MONTO,(CASE 
				WHEN T0."DocCur" in ('USD','EUR') 
					THEN T1."InsTotalFC" - T1."PaidFC"
					- T0."WTSumFC"
				ELSE 
					T1."InsTotal" - T1."PaidToDate" 
					- T0."WTSum"
			END))AS DECIMAL(16,2))
		AS VARCHAR),16,'0')
		AS "ImporteParcial",
		LPAD(CAST(CAST(
			T6."MontototalSum"
		AS DECIMAL(16,2))AS VARCHAR),16,'0')
		AS "Importetotal",
		'S' AS "ValidacionIDC",
		'F' AS "TipoDocumentoPagar",
		/*
		LPAD(SUBSTRING(T0."NumAtCard",0,LOCATE(T0."NumAtCard",'-')-1) || 
		LPAD(SUBSTRING(T0."NumAtCard",LOCATE(T0."NumAtCard",'-')+1,LENGTH(T0."NumAtCard")-LOCATE(T0."NumAtCard",'-')+1),7,'0')
		,15,'0')
		--LPAD(REPLACE(T0."NumAtCard",'-',''),15,'0') */
		LPAD(IFNULL(REPLACE(T0."NumAtCard",'-',''),'0'),10,'0')
		AS "NumeroDocumento"
		,T0."FolioPref" as "SerieDocumento"
		,TO_VARCHAR(T0."DocDueDate",'YYYYMMDD') as "FechaVencimiento"
		,(CASE WHEN T3."U_EXX_TIPOPERS" = 'TPJ' THEN 'C'
				WHEN T3."U_EXX_TIPOPERS" = 'TPN' THEN 'P' END) as "TipoPersona"
		
	FROM 
		ORIN T0
		LEFT JOIN RIN6 T1 ON T0."DocEntry" = T1."DocEntry" AND T1."InstlmntID" = '1'
		LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
		LEFT JOIN OCRD T3 ON T0."CardCode" = T3."CardCode"
		LEFT JOIN "@SMC_APM_ESCDET" T4 
		ON T0."DocEntry" = T4."U_SMC_DOCENTRY" 
		AND T1."DocEntry" = T4."U_SMC_DOCENTRY"
		AND T4."U_SMC_TIPO_DOCUMENTO" = 'NC-C'
	    LEFT JOIN "@SMC_APM_ESCCAB" T5 ON T5."Code" = T4."U_SMC_ESCCAB" 
	    LEFT JOIN "#tmpTotalDetSum" T6 ON T3."CardCode" = T6."Proveedor"
	WHERE 
		T0."Indicator" IN ('00','01','02','14','50','99','05','07')
		AND T1."InstlmntID" = '1'
		AND T2."PymntGroup" not like '%DT%'
		AND IFNULL(T4."U_SMC_PAGO",'N') = 'Y'
		AND IFNULL(T4."U_SMC_REG",'N') = 'N'
		AND T5."Code" = :escenario
		AND T4."U_SMC_TIPO_DOCUMENTO" = 'NC-C'
		--AND T0."DocTotal" = T4."U_SMC_MONTO"
		AND T0."DocStatus" != 'C'

		
		
	UNION
	
	
	SELECT
		'2' AS "TipoRegistro",
		/*
	 	CASE(IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y' )
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '004' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,''))
			WHEN '003' THEN 
				CASE LENGTH(
					TRIM(IFNULL(
					(SELECT MAX(R0."Account") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur")
					,'')
					))
					WHEN 14 THEN 'C'
					ELSE 'I'
				END
			ELSE 'I'
		END*/
		(SELECT (CASE WHEN MAX(R0."Account") is null then 'I' else 'C' end) FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode"
		AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y')
		AS "TipoCuenta",
		RPAD((IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		(SELECT MAX(R0."Account") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '004' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,'9')),20)
		AS "CuentaAbono",
		'1' AS "Caracter",
		(CASE WHEN T3."U_EXX_TIPODOCU" = '6' then '02'
			WHEN T3."U_EXX_TIPODOCU" = '1'then '01' END) AS "TipoDocumentoIdentidad",
		--T3."U_EXX_TIPODOCU" AS "TipoDocumentoIdentidad",
		RPAD(T3."LicTradNum",15) AS "NumeroDocumentoIdentidad",
		RPAD(T3."LicTradNum",20) AS "NumeroDocumentoIdentidad1",
		
		RPAD(T3."CardName",60) AS "NombreProveedor",
		RPAD('Referencia Beneficiario ' || T3."LicTradNum",40) AS "ReferenciaBeneficiario",
		RPAD('Ref Emp ' || T3."LicTradNum",20) AS "Referencia",
		CASE
			WHEN T0."DocCur" = 'SOL' THEN '01'
			ELSE '10' 
		END AS "Moneda",
		LPAD(CAST(
			CAST(IFNULL(T4.U_SMC_MONTO,(CASE 
				WHEN T0."DocCur" in ('USD','EUR') 
					THEN T1."InsTotalFC" - T1."PaidFC"
					- T0."WTSumFC"
				ELSE 
					T1."InsTotal" - T1."PaidToDate" 
					- T0."WTSum"
			END))AS DECIMAL(16,2))
		AS VARCHAR),16,'0')
		AS "ImporteParcial",
		LPAD(CAST(CAST(
			T6."MontototalSum"
		AS DECIMAL(16,2))AS VARCHAR),16,'0')
		AS "Importetotal",
		'S' AS "ValidacionIDC",
		'F' AS "TipoDocumentoPagar",
		/*
		LPAD(SUBSTRING(T0."NumAtCard",0,LOCATE(T0."NumAtCard",'-')-1) || 
		LPAD(SUBSTRING(T0."NumAtCard",LOCATE(T0."NumAtCard",'-')+1,LENGTH(T0."NumAtCard")-LOCATE(T0."NumAtCard",'-')+1),7,'0')
		,15,'0')
		--LPAD(REPLACE(T0."NumAtCard",'-',''),15,'0') */
		LPAD(IFNULL(REPLACE(T0."NumAtCard",'-',''),'0'),10,'0')
		AS "NumeroDocumento"
		,T0."FolioPref" as "SerieDocumento"
		,TO_VARCHAR(T0."DocDueDate",'YYYYMMDD') as "FechaVencimiento"
		,(CASE WHEN T3."U_EXX_TIPOPERS" = 'TPJ' THEN 'C'
				WHEN T3."U_EXX_TIPOPERS" = 'TPN' THEN 'P' END) as "TipoPersona"
		
		
	FROM 
		ODPO T0
		LEFT JOIN DPO6 T1 ON T0."DocEntry" = T1."DocEntry" AND T1."InstlmntID" = '1'
		LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
		LEFT JOIN OCRD T3 ON T0."CardCode" = T3."CardCode"
		LEFT JOIN "@SMC_APM_ESCDET" T4 
		ON T0."DocEntry" = T4."U_SMC_DOCENTRY" 
		AND T1."DocEntry" = T4."U_SMC_DOCENTRY"
		AND T4."U_SMC_TIPO_DOCUMENTO" = 'FA-P'
	    LEFT JOIN "@SMC_APM_ESCCAB" T5 ON T5."Code" = T4."U_SMC_ESCCAB" 
	    LEFT JOIN "#tmpTotalDetSum" T6 ON T3."CardCode" = T6."Proveedor"
	WHERE 
		T0."Indicator" IN ('00','01','02','14','50','99','05','07','DI')
		AND T1."InstlmntID" = '1'
		AND T0."CreateTran" = 'Y'
		AND T2."PymntGroup" not like '%DT%'
		AND IFNULL(T4."U_SMC_PAGO",'N') = 'Y'
		AND IFNULL(T4."U_SMC_REG",'N') = 'N'
		AND T5."Code" = :escenario
		AND T4."U_SMC_TIPO_DOCUMENTO" = 'FA-P'
		--AND T0."DocTotal" = T4."U_SMC_MONTO"
		AND T0."DocStatus" != 'C'
		
		
		
		UNION
	
	
	SELECT
		'2' AS "TipoRegistro",
		/*
	 	CASE(IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '004' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,''))
			WHEN '003' THEN 
				CASE LENGTH(
					TRIM(IFNULL(
					(SELECT MAX(R0."Account") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur")
					,'')
					))
					WHEN 14 THEN 'C'
					ELSE 'I'
				END
			ELSE 'I'
		END*/
		(SELECT (CASE WHEN MAX(R0."Account") is null then 'I' else 'C' end) FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode"
		AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y')
		AS "TipoCuenta",
		
		RPAD((IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		(SELECT MAX(R0."Account") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '004' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur" and "U_EXC_ACTIVO" = 'Y'))
		,'9')),20)
		AS "CuentaAbono",
		'1' AS "Caracter",
		(CASE WHEN T3."U_EXX_TIPODOCU" = '6' then '02'
			WHEN T3."U_EXX_TIPODOCU" = '1'then '01' END) AS "TipoDocumentoIdentidad",
		--T3."U_EXX_TIPODOCU" AS "TipoDocumentoIdentidad",
		RPAD(T3."LicTradNum",15) AS "NumeroDocumentoIdentidad",
		RPAD(T3."LicTradNum",20) AS "NumeroDocumentoIdentidad1",
		
		RPAD(T3."CardName",60) AS "NombreProveedor",
		RPAD('Referencia Beneficiario ' || T3."LicTradNum",40) AS "ReferenciaBeneficiario",
		RPAD('Ref Emp ' || T3."LicTradNum",20) AS "Referencia",
		CASE
			WHEN T0."DocCur" = 'SOL' THEN '01'
			ELSE '10' 
		END AS "Moneda",
		LPAD(CAST(
			CAST(IFNULL(T4.U_SMC_MONTO,(CASE 
				WHEN T0."DocCur" in ('USD','EUR') 
					THEN T1."InsTotalFC" - T1."PaidFC"
					- T0."WTSumFC"
				ELSE 
					T1."InsTotal" - T1."PaidToDate" 
					- T0."WTSum"
			END))AS DECIMAL(16,2))
		AS VARCHAR),16,'0')
		AS "ImporteParcial",
		LPAD(CAST(CAST(
			T6."MontototalSum"
		AS DECIMAL(16,2))AS VARCHAR),16,'0')
		AS "Importetotal",
		'S' AS "ValidacionIDC",
		'F' AS "TipoDocumentoPagar",
		/*
		LPAD(SUBSTRING(T0."NumAtCard",0,LOCATE(T0."NumAtCard",'-')-1) || 
		LPAD(SUBSTRING(T0."NumAtCard",LOCATE(T0."NumAtCard",'-')+1,LENGTH(T0."NumAtCard")-LOCATE(T0."NumAtCard",'-')+1),7,'0')
		,15,'0')
		--LPAD(REPLACE(T0."NumAtCard",'-',''),15,'0') */
		LPAD(IFNULL(REPLACE(T0."NumAtCard",'-',''),'0'),10,'0')
		AS "NumeroDocumento"
		,T0."FolioPref" as "SerieDocumento"
		,TO_VARCHAR(T0."DocDueDate",'YYYYMMDD') as "FechaVencimiento"
		,(CASE WHEN T3."U_EXX_TIPOPERS" = 'TPJ' THEN 'C'
				WHEN T3."U_EXX_TIPOPERS" = 'TPN' THEN 'P' END) as "TipoPersona"
	FROM 
		ODPO T0
		LEFT JOIN DPO6 T1 ON T0."DocEntry" = T1."DocEntry" AND T1."InstlmntID" = '1'
		LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
		LEFT JOIN OCRD T3 ON T0."CardCode" = T3."CardCode"
		LEFT JOIN "@SMC_APM_ESCDET" T4 
		ON T0."DocEntry" = T4."U_SMC_DOCENTRY" 
		AND T1."DocEntry" = T4."U_SMC_DOCENTRY"
		AND T4."U_SMC_TIPO_DOCUMENTO" = 'SA-P'
	    LEFT JOIN "@SMC_APM_ESCCAB" T5 ON T5."Code" = T4."U_SMC_ESCCAB" 
	    LEFT JOIN "#tmpTotalDetSum" T6 ON T3."CardCode" = T6."Proveedor"
	WHERE 
		T0."Indicator" IN ('00','01','02','14','50','99','05','07','DI')
		AND T1."InstlmntID" = '1'
		AND T0."CreateTran" = 'N'
		AND T2."PymntGroup" not like '%DT%'
		AND IFNULL(T4."U_SMC_PAGO",'N') = 'Y'
		AND IFNULL(T4."U_SMC_REG",'N') = 'N'
		AND T5."Code" = :escenario
		AND T4."U_SMC_TIPO_DOCUMENTO" = 'SA-P'
		--AND T0."DocTotal" = T4."U_SMC_MONTO"
		AND T0."DocStatus" != 'C'



	
	
		UNION
	
	
	SELECT
		'2' AS "TipoRegistro",
		/*
	 	CASE(IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCurr" and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCurr" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCurr" and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCurr" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '004' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCurr" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCurr" and "U_EXC_ACTIVO" = 'Y'))
		,''))
			WHEN '003' THEN 
				CASE LENGTH(
					TRIM(IFNULL(
					(SELECT MAX(R0."Account") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCurr")
					,'')
					))
					WHEN 14 THEN 'C'
					ELSE 'I'
				END
			ELSE 'I'
		END*/
		(SELECT (CASE WHEN MAX(R0."Account") is null then 'I' else 'C' end) FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode"
		AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCurr" and "U_EXC_ACTIVO" = 'Y')
		AS "TipoCuenta",
		
		RPAD((IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		(SELECT MAX(R0."Account") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCurr" and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCurr" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCurr" and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCurr" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '004' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCurr" and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCurr" and "U_EXC_ACTIVO" = 'Y'))
		,'9')),20)
		AS "CuentaAbono",
		'1' AS "Caracter",
		(CASE WHEN T3."U_EXX_TIPODOCU" = '6' then '02'
			WHEN T3."U_EXX_TIPODOCU" = '1'then '01' END) AS "TipoDocumentoIdentidad",
		--T3."U_EXX_TIPODOCU" AS "TipoDocumentoIdentidad",
		RPAD(T3."LicTradNum",15) AS "NumeroDocumentoIdentidad",
		RPAD(T3."LicTradNum",20) AS "NumeroDocumentoIdentidad1",
		
		RPAD(T3."CardName",60) AS "NombreProveedor",
		RPAD('Referencia Beneficiario ' || T3."LicTradNum",40) AS "ReferenciaBeneficiario",
		RPAD('Ref Emp ' || T3."LicTradNum",20) AS "Referencia",
		CASE
			WHEN T0."DocCurr" = 'SOL' THEN '01'
			ELSE '10' 
		END AS "Moneda",
		
		
		LPAD(CAST( 
			(CASE 
				WHEN T0."DocCurr" in ('USD','EUR') 
					THEN T0."NoDocSumFC" 
				ELSE 
					T0."NoDocSum"
			END)AS DECIMAL(16,2)),16,'0')
		AS "ImporteParcial",
		
		
		LPAD(CAST( 
			(CASE 
				WHEN T0."DocCurr" in ('USD','EUR') 
					THEN T0."NoDocSumFC" 
				ELSE 
					T0."NoDocSum"
			END)AS DECIMAL(16,2)),16,'0')
		AS "Importetotal",
		
		
		
		'S' AS "ValidacionIDC",
		'D' AS "TipoDocumentoPagar",
		
		/*LPAD(SUBSTRING(T0."NumAtCard",0,LOCATE(T0."NumAtCard",'-')-1) || 
		LPAD(SUBSTRING(T0."NumAtCard",LOCATE(T0."NumAtCard",'-')+1,LENGTH(T0."NumAtCard")-LOCATE(T0."NumAtCard",'-')+1),7,'0')
		,15,'0')*/
		LPAD(IFNULL(REPLACE(T0."DocNum",'-',''),'0'),10,'0')
		AS "NumeroDocumento"
		
		,'' as "SerieDocumento"
		,TO_VARCHAR(T0."DocDueDate",'YYYYMMDD') as "FechaVencimiento"
		,(CASE WHEN T3."U_EXX_TIPOPERS" = 'TPJ' THEN 'C'
				WHEN T3."U_EXX_TIPOPERS" = 'TPN' THEN 'P' END) as "TipoPersona"
		
	FROM 
		OPDF T0
		--LEFT JOIN DPO6 T1 ON T0."DocEntry" = T1."DocEntry" AND T1."InstlmntID" = '1'
		--LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
		LEFT JOIN OCRD T3 ON T0."CardCode" = T3."CardCode"
		LEFT JOIN "@SMC_APM_ESCDET" T4 
		ON T0."DocEntry" = T4."U_SMC_DOCENTRY" 
		--AND T1."DocEntry" = T4."U_SMC_DOCENTRY"
		AND T4."U_SMC_TIPO_DOCUMENTO" = 'SP'
	    LEFT JOIN "@SMC_APM_ESCCAB" T5 ON T5."Code" = T4."U_SMC_ESCCAB" 
	    LEFT JOIN "#tmpTotalDetSum" T6 ON T3."CardCode" = T6."Proveedor"
	WHERE 
		--T0."Indicator" IN ('01','02','14','50','99','05','07','DI')
		--AND T1."InstlmntID" = '1'
		--AND T0."CreateTran" = 'N'
		--AND T2."PymntGroup" not like '%DT%'
		IFNULL(T4."U_SMC_PAGO",'N') = 'Y'
		AND IFNULL(T4."U_SMC_REG",'N') = 'N'
		AND T5."Code" = :escenario
		AND T4."U_SMC_TIPO_DOCUMENTO" = 'SP'
		--AND T0."DocTotal" = T4."U_SMC_MONTO"
	




		UNION
	
	
	
	
	SELECT
		'2' AS "TipoRegistro",
		/*
	 	CASE(IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = IFNULL(T1."FCCurrency",'SOL') and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '011' AND IFNULL(R0."UsrNumber1",'SOL') = IFNULL(T1."FCCurrency",'SOL') and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '022' AND IFNULL(R0."UsrNumber1",'SOL') = IFNULL(T1."FCCurrency",'SOL') and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '009' AND IFNULL(R0."UsrNumber1",'SOL') = IFNULL(T1."FCCurrency",'SOL') and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '004' AND IFNULL(R0."UsrNumber1",'SOL') = IFNULL(T1."FCCurrency",'SOL') and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '002' AND IFNULL(R0."UsrNumber1",'SOL') = IFNULL(T1."FCCurrency",'SOL') and "U_EXC_ACTIVO" = 'Y'))
		,''))
			WHEN '003' THEN 
				CASE LENGTH(
					TRIM(IFNULL(
					(SELECT MAX(R0."Account") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = IFNULL(T1."FCCurrency",'SOL'))
					,'')
					))
					WHEN 14 THEN 'C'
					ELSE 'I'
				END
			ELSE 'I'
		END*/
		(SELECT (CASE WHEN MAX(R0."Account") is null then 'I' else 'C' end) FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode"
		AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = T1."FCCurrency" and "U_EXC_ACTIVO" = 'Y')
		AS "TipoCuenta",
		RPAD((IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		(SELECT MAX(R0."Account") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = IFNULL(T1."FCCurrency",'SOL') and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '011' AND IFNULL(R0."UsrNumber1",'SOL') = IFNULL(T1."FCCurrency",'SOL') and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '022' AND IFNULL(R0."UsrNumber1",'SOL') = IFNULL(T1."FCCurrency",'SOL') and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '009' AND IFNULL(R0."UsrNumber1",'SOL') = IFNULL(T1."FCCurrency",'SOL') and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '004' AND IFNULL(R0."UsrNumber1",'SOL') = IFNULL(T1."FCCurrency",'SOL') and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '002' AND IFNULL(R0."UsrNumber1",'SOL') = IFNULL(T1."FCCurrency",'SOL') and "U_EXC_ACTIVO" = 'Y'))
		,'9')),20)
		AS "CuentaAbono",
		'1' AS "Caracter",
		(CASE WHEN T3."U_EXX_TIPODOCU" = '6' then '02'
			WHEN T3."U_EXX_TIPODOCU" = '1'then '01' END) AS "TipoDocumentoIdentidad",
		--T3."U_EXX_TIPODOCU" AS "TipoDocumentoIdentidad",
		RPAD(T3."LicTradNum",15) AS "NumeroDocumentoIdentidad",
		RPAD(T3."LicTradNum",20) AS "NumeroDocumentoIdentidad1",
		
		RPAD(T3."CardName",60) AS "NombreProveedor",
		RPAD('Referencia Beneficiario ' || T3."LicTradNum",40) AS "ReferenciaBeneficiario",
		RPAD('Ref Emp ' || T3."LicTradNum",20) AS "Referencia",
		CASE
			WHEN ifnull(T1."FCCurrency",'SOL') = 'SOL' THEN '01'
			ELSE '10' 
		END AS "Moneda",
		
		
		LPAD(CAST( 
		(CASE 
			WHEN ifnull(T1."FCCurrency",'SOL') in ('USD','EUR') 
				THEN 
				(T0."FcTotal" - ifnull((select sum(TT0."ReconSumFC") from 
				"ITR1" TT0 where TT0."TransId" = T0."TransId"
				GROUP BY TT0."TransId"),0)) 

			ELSE 
			(T0."LocTotal" - ifnull((select sum(TT0."ReconSum") from 
				"ITR1" TT0 where TT0."TransId" = T0."TransId"
				GROUP BY TT0."TransId"),0))
		END)AS DECIMAL(16,2)),16,'0')
		AS "ImporteParcial",
		
		
		LPAD(CAST( 
		(CASE 
			WHEN ifnull(T1."FCCurrency",'SOL') in ('USD','EUR') 
				THEN (T0."FcTotal" - ifnull((select sum(TT0."ReconSumFC") from 
				"ITR1" TT0 where TT0."TransId" = T0."TransId"
				GROUP BY TT0."TransId"),0)) 
			ELSE 
				(T0."LocTotal" - ifnull((select sum(TT0."ReconSum") from 
				"ITR1" TT0 where TT0."TransId" = T0."TransId"
				GROUP BY TT0."TransId"),0))
		END)AS DECIMAL(16,2)),16,'0')
		AS "Importetotal",
		
		
		
		'S' AS "ValidacionIDC",
		'D' AS "TipoDocumentoPagar",
		

		LPAD(IFNULL(REPLACE(T0."BaseRef",'-',''),'0'),10,'0')
		AS "NumeroDocumento"
		,'' as "SerieDocumento"
		,TO_VARCHAR(T0."DueDate",'YYYYMMDD') as "FechaVencimiento"
		,(CASE WHEN T3."U_EXX_TIPOPERS" = 'TPJ' THEN 'C'
				WHEN T3."U_EXX_TIPOPERS" = 'TPN' THEN 'P' END) as "TipoPersona"
		
	FROM 
		OJDT T0
		--LEFT JOIN DPO6 T1 ON T0."DocEntry" = T1."DocEntry" AND T1."InstlmntID" = '1'
		LEFT JOIN JDT1 T1 ON T0."TransId" = T1."TransId"
		--LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
		inner JOIN OCRD T3 ON T1."ShortName" = T3."CardCode"
		LEFT JOIN "@SMC_APM_ESCDET" T4 
		ON T0."TransId" = T4."U_SMC_DOCENTRY" 
		--AND T1."DocEntry" = T4."U_SMC_DOCENTRY"
		AND T4."U_SMC_TIPO_DOCUMENTO" = 'AS'
	    LEFT JOIN "@SMC_APM_ESCCAB" T5 ON T5."Code" = T4."U_SMC_ESCCAB" 
	    LEFT JOIN "#tmpTotalDetSum" T6 ON T3."CardCode" = T6."Proveedor"
	WHERE 
		--T0."Indicator" IN ('01','02','14','50','99','05','07','DI')
		--AND T1."InstlmntID" = '1'
		--AND T0."CreateTran" = 'N'
		--AND T2."PymntGroup" not like '%DT%'
		IFNULL(T4."U_SMC_PAGO",'N') = 'Y'
		AND IFNULL(T4."U_SMC_REG",'N') = 'N'
		AND T5."Code" = :escenario
		and T1."Credit" > 0
		AND T4."U_SMC_TIPO_DOCUMENTO" = 'AS'
		--AND T0."DocTotal" = T4."U_SMC_MONTO"
		and (CASE 
			WHEN ifnull(T1."FCCurrency",'SOL') in ('USD','EUR') 
				THEN (T0."FcTotal" - ifnull((select sum(TT0."ReconSumFC") from 
				"ITR1" TT0 where TT0."TransId" = T0."TransId"
				GROUP BY TT0."TransId"),0)) 
			ELSE 
				(T0."LocTotal" - ifnull((select sum(TT0."ReconSum") from 
				"ITR1" TT0 where TT0."TransId" = T0."TransId"
				GROUP BY TT0."TransId"),0))
		END)>0
		
		


		union
		
		
		
			
	
	SELECT
		'2' AS "TipoRegistro",
		/*
	 	CASE(IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = IFNULL(T1."FCCurrency",'SOL') and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '011' AND IFNULL(R0."UsrNumber1",'SOL') = IFNULL(T1."FCCurrency",'SOL') and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '022' AND IFNULL(R0."UsrNumber1",'SOL') = IFNULL(T1."FCCurrency",'SOL') and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '009' AND IFNULL(R0."UsrNumber1",'SOL') = IFNULL(T1."FCCurrency",'SOL') and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '004' AND IFNULL(R0."UsrNumber1",'SOL') = IFNULL(T1."FCCurrency",'SOL') and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '002' AND IFNULL(R0."UsrNumber1",'SOL') = IFNULL(T1."FCCurrency",'SOL') and "U_EXC_ACTIVO" = 'Y'))
		,''))
			WHEN '003' THEN 
				CASE LENGTH(
					TRIM(IFNULL(
					(SELECT MAX(R0."Account") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = IFNULL(T1."FCCurrency",'SOL'))
					,'')
					))
					WHEN 14 THEN 'C'
					ELSE 'I'
				END
			ELSE 'I'
		END*/
		(SELECT (CASE WHEN MAX(R0."Account") is null then 'I' else 'C' end) FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode"
		AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = T1."FCCurrency" and "U_EXC_ACTIVO" = 'Y')
		AS "TipoCuenta",
		RPAD((IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		(SELECT MAX(R0."Account") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '003' AND IFNULL(R0."UsrNumber1",'SOL') = IFNULL(T1."FCCurrency",'SOL') and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '011' AND IFNULL(R0."UsrNumber1",'SOL') = IFNULL(T1."FCCurrency",'SOL') and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '022' AND IFNULL(R0."UsrNumber1",'SOL') = IFNULL(T1."FCCurrency",'SOL') and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '009' AND IFNULL(R0."UsrNumber1",'SOL') = IFNULL(T1."FCCurrency",'SOL') and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '004' AND IFNULL(R0."UsrNumber1",'SOL') = IFNULL(T1."FCCurrency",'SOL') and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '002' AND IFNULL(R0."UsrNumber1",'SOL') = IFNULL(T1."FCCurrency",'SOL') and "U_EXC_ACTIVO" = 'Y'))
		,'9')),20)
		AS "CuentaAbono",
		'1' AS "Caracter",
		(CASE WHEN T3."U_EXX_TIPODOCU" = '6' then '02'
			WHEN T3."U_EXX_TIPODOCU" = '1'then '01' END) AS "TipoDocumentoIdentidad",
		--T3."U_EXX_TIPODOCU" AS "TipoDocumentoIdentidad",
		RPAD(T3."LicTradNum",15) AS "NumeroDocumentoIdentidad",
		RPAD(T3."LicTradNum",20) AS "NumeroDocumentoIdentidad1",
		
		RPAD(T3."CardName",60) AS "NombreProveedor",
		RPAD('Referencia Beneficiario ' || T3."LicTradNum",40) AS "ReferenciaBeneficiario",
		RPAD('Ref Emp ' || T3."LicTradNum",20) AS "Referencia",
		CASE
			WHEN ifnull(T1."FCCurrency",'SOL') = 'SOL' THEN '01'
			ELSE '10' 
		END AS "Moneda",
		
		
		LPAD(CAST( 
		(CASE 
			WHEN ifnull(T1."FCCurrency",'SOL') in ('USD','EUR') 
				THEN 	(T0."FcTotal" - ifnull((select sum(TT0."ReconSumFC") from 
				"ITR1" TT0 where TT0."TransId" = T0."TransId"
				GROUP BY TT0."TransId"),0)) 
			ELSE 
				(T0."LocTotal" - ifnull((select sum(TT0."ReconSum") from 
				"ITR1" TT0 where TT0."TransId" = T0."TransId"
				GROUP BY TT0."TransId"),0))
		END)AS DECIMAL(16,2)),16,'0')
		AS "ImporteParcial",
		
		
		LPAD(CAST( 
		(CASE 
			WHEN ifnull(T1."FCCurrency",'SOL') in ('USD','EUR') 
				THEN (T0."FcTotal" - ifnull((select sum(TT0."ReconSumFC") from 
				"ITR1" TT0 where TT0."TransId" = T0."TransId"
				GROUP BY TT0."TransId"),0)) 
			ELSE 
				(T0."LocTotal" - ifnull((select sum(TT0."ReconSum") from 
				"ITR1" TT0 where TT0."TransId" = T0."TransId"
				GROUP BY TT0."TransId"),0))
		END)AS DECIMAL(16,2)),16,'0')
		AS "Importetotal",
		
		
		
		'S' AS "ValidacionIDC",
		'D' AS "TipoDocumentoPagar",
		

		LPAD(IFNULL(REPLACE(T0."BaseRef",'-',''),'0'),10,'0')
		AS "NumeroDocumento"
		,'' as "SerieDocumento"
		,TO_VARCHAR(T0."DueDate",'YYYYMMDD') as "FechaVencimiento"
		,(CASE WHEN T3."U_EXX_TIPOPERS" = 'TPJ' THEN 'C'
				WHEN T3."U_EXX_TIPOPERS" = 'TPN' THEN 'P' END) as "TipoPersona"
		
	FROM 
		OJDT T0
		--LEFT JOIN DPO6 T1 ON T0."DocEntry" = T1."DocEntry" AND T1."InstlmntID" = '1'
		LEFT JOIN JDT1 T1 ON T0."TransId" = T1."TransId"
		--LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
		inner JOIN OCRD T3 ON T1."ShortName" = T3."CardCode"
		LEFT JOIN "@SMC_APM_ESCDET" T4 
		ON T0."TransId" = T4."U_SMC_DOCENTRY" 
		--AND T1."DocEntry" = T4."U_SMC_DOCENTRY"
		AND T4."U_SMC_TIPO_DOCUMENTO" = 'PR'
	    LEFT JOIN "@SMC_APM_ESCCAB" T5 ON T5."Code" = T4."U_SMC_ESCCAB" 
	    LEFT JOIN "#tmpTotalDetSum" T6 ON T3."CardCode" = T6."Proveedor"
	WHERE 
		--T0."Indicator" IN ('01','02','14','50','99','05','07','DI')
		--AND T1."InstlmntID" = '1'
		--AND T0."CreateTran" = 'N'
		--AND T2."PymntGroup" not like '%DT%'
		IFNULL(T4."U_SMC_PAGO",'N') = 'Y'
		AND IFNULL(T4."U_SMC_REG",'N') = 'N'
		AND T5."Code" = :escenario
		and T1."Credit" > 0
		AND T4."U_SMC_TIPO_DOCUMENTO" = 'PR'
		--AND T0."DocTotal" = T4."U_SMC_MONTO"
		and
		(CASE 
			WHEN ifnull(T1."FCCurrency",'SOL') in ('USD','EUR') 
				THEN (T0."FcTotal" - ifnull((select sum(TT0."ReconSumFC") from 
				"ITR1" TT0 where TT0."TransId" = T0."TransId"
				GROUP BY TT0."TransId"),0)) 
			ELSE 
				(T0."LocTotal" - ifnull((select sum(TT0."ReconSum") from 
				"ITR1" TT0 where TT0."TransId" = T0."TransId"
				GROUP BY TT0."TransId"),0))
		END) > 0

		
		
	ORDER BY
		2)as TT where "CuentaAbono" != '9';
		
	DROP TABLE "#tmpTotalDetSum";
END;