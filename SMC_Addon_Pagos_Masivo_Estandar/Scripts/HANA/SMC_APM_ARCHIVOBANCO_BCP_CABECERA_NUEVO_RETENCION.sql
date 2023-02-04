CREATE PROCEDURE "SMC_APM_ARCHIVOBANCO_BCP_CABECERA_NUEVO_RETENCION"
(
	DocEntryPM INT,
	Moneda nvarchar(3)
)
AS
BEGIN	 

	lt_data_tmp_1 =(
		SELECT 
			SUM(Z0."Montototal") AS "MontototalSum",
			SUM(Z0."Cuenta") AS "TotalCuenta"
		FROM
		(SELECT
			CAST(IFNULL(T4."U_EXP_IMPORTE",(CASE 
				WHEN T0."DocCur" in ('USD','EUR') 
					THEN T1."InsTotalFC" - T1."PaidFC"
					- T0."WTSumFC"
				ELSE 
					T1."InsTotal" - T1."PaidToDate" 
					- T0."WTSum"
			END))AS DECIMAL(16,2))
			AS "Montototal",
			
			CAST(
			ifnull((SELECT MAX(R0."UsrNumber3") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur"and "U_EXC_ACTIVO" = 'Y' ),'0')
			AS BIGINT)
			AS "Cuenta"
		FROM 
			OPCH T0
			LEFT JOIN PCH6 T1 ON T0."DocEntry" = T1."DocEntry"
			LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
			LEFT JOIN "@EXP_PMP1" T4
			ON T0."DocEntry" = T4."U_EXP_DOCENTRYDOC"
			AND T1."DocEntry" = T4."U_EXP_DOCENTRYDOC"
			--AND T4."U_SMC_TIPO_DOCUMENTO" = 'FT-P'
		WHERE
			T0."Indicator" IN ('00','01','02','14','50','99','05')
			AND T1."InstlmntID" = '1'
			AND T2."PymntGroup" like '%DT%'
			AND IFNULL(T4."U_EXP_SLC_PAGO",'N') = 'Y'
			AND IFNULL(T4."U_EXP_SLC_RETENCION",'N') = 'Y'
			AND T4."DocEntry"= :DocEntryPM
			--AND T4."U_SMC_TIPO_DOCUMENTO" = 'FT-P'
			--and T0."DocTotal" = T4."U_SMC_MONTO"
			AND T0."DocStatus" != 'C'
			
		UNION
		
		SELECT
			CAST(IFNULL(T4."U_EXP_IMPORTE",(CASE 
				WHEN T0."DocCur" in ('USD','EUR') 
					THEN T1."InsTotalFC" - T1."PaidFC"
					- T0."WTSumFC"
				ELSE 
					T1."InsTotal" - T1."PaidToDate" 
					- T0."WTSum"
			END))AS DECIMAL(16,2))
			AS "Montototal",
			
			CAST(
			ifnull((SELECT MAX(R0."UsrNumber3") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur"and "U_EXC_ACTIVO" = 'Y' ),'0')
			AS BIGINT)
			AS "Cuenta"
		FROM 
			OPCH T0
			LEFT JOIN PCH6 T1 ON T0."DocEntry" = T1."DocEntry" AND T1."InstlmntID" = '1'
			LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
			LEFT JOIN "@EXP_PMP1" T4
			ON T0."DocEntry" = T4."U_EXP_DOCENTRYDOC"
			AND T1."DocEntry" = T4."U_EXP_DOCENTRYDOC"
			--AND T4."U_SMC_TIPO_DOCUMENTO" = 'FT-P'
		WHERE
			T0."Indicator" IN ('00','01','02','14','50','99','05')
			AND T1."InstlmntID" = '1'
			AND T2."PymntGroup" not like '%DT%'
			AND IFNULL(T4."U_EXP_SLC_PAGO",'N') = 'Y'
			AND IFNULL(T4."U_EXP_SLC_RETENCION",'N') = 'Y'
			AND T4."DocEntry"= :DocEntryPM
			--AND T4."U_SMC_TIPO_DOCUMENTO" = 'FT-P'
			--and T0."DocTotal" = T4."U_SMC_MONTO"
			AND T0."DocStatus" != 'C'
			
			union
			
			--nuevo

			SELECT
			CAST(IFNULL(T4."U_EXP_IMPORTE",(CASE 
				WHEN T0."DocCur" in ('USD','EUR') 
					THEN T1."InsTotalFC" - T1."PaidFC"
					- T0."WTSumFC"
				ELSE 
					T1."InsTotal" - T1."PaidToDate" 
					- T0."WTSum"
			END))AS DECIMAL(16,2))
			AS "Montototal",
			CAST(
			ifnull((SELECT MAX(R0."UsrNumber3") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur"and "U_EXC_ACTIVO" = 'Y' ),'0')
			AS BIGINT)
			AS "Cuenta"
		FROM 
			ORIN T0
			LEFT JOIN RIN6 T1 ON T0."DocEntry" = T1."DocEntry" AND T1."InstlmntID" = '1'
			LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
			LEFT JOIN "@EXP_PMP1" T4
			ON T0."DocEntry" = T4."U_EXP_DOCENTRYDOC"
			AND T1."DocEntry" = T4."U_EXP_DOCENTRYDOC"
			--AND T4."U_SMC_TIPO_DOCUMENTO" = 'NC-C'
		WHERE
			T0."Indicator" IN ('00','01','02','14','50','99','05','07')
			AND T1."InstlmntID" = '1'
			AND T2."PymntGroup" not like '%DT%'
			AND IFNULL(T4."U_EXP_SLC_PAGO",'N') = 'Y'
			AND IFNULL(T4."U_EXP_SLC_RETENCION",'N') = 'Y'
			AND T4."DocEntry"= :DocEntryPM
			--AND T4."U_SMC_TIPO_DOCUMENTO" = 'NC-C'
			--and T0."DocTotal" = T4."U_SMC_MONTO"
			AND T0."DocStatus" != 'C'
			
			union
			
			
			SELECT
			CAST(IFNULL(T4."U_EXP_IMPORTE",(CASE 
				WHEN T0."DocCur" in ('USD','EUR') 
					THEN T1."InsTotalFC" - T1."PaidFC"
					- T0."WTSumFC"
				ELSE 
					T1."InsTotal" - T1."PaidToDate" 
					- T0."WTSum"
			END))AS DECIMAL(16,2))
			AS "Montototal",
			CAST(
			ifnull((SELECT MAX(R0."UsrNumber3") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur"and "U_EXC_ACTIVO" = 'Y' ),'0')
			AS BIGINT)
			AS "Cuenta"
		FROM 
			ODPO T0
			LEFT JOIN DPO6 T1 ON T0."DocEntry" = T1."DocEntry" AND T1."InstlmntID" = '1'
			LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
			LEFT JOIN "@EXP_PMP1" T4
			ON T0."DocEntry" = T4."U_EXP_DOCENTRYDOC"
			AND T1."DocEntry" = T4."U_EXP_DOCENTRYDOC"
			--AND T4."U_SMC_TIPO_DOCUMENTO" = 'FA-P'
		WHERE
			T0."Indicator" IN ('00','01','02','14','50','99','05','DI')
			AND T1."InstlmntID" = '1'
			AND T0."CreateTran" = 'Y'
			AND T2."PymntGroup" not like '%DT%'
			AND IFNULL(T4."U_EXP_SLC_PAGO",'N') = 'Y'
			AND IFNULL(T4."U_EXP_SLC_RETENCION",'N') = 'Y'
			AND T4."DocEntry"= :DocEntryPM
			--AND T4."U_SMC_TIPO_DOCUMENTO" = 'FA-P'
			--and T0."DocTotal" = T4."U_SMC_MONTO"
			AND T0."DocStatus" != 'C'
			
			
			union
			
			
			SELECT
			CAST(IFNULL(T4."U_EXP_IMPORTE",(CASE 
				WHEN T0."DocCur" in ('USD','EUR') 
					THEN T1."InsTotalFC" - T1."PaidFC"
					- T0."WTSumFC"
				ELSE 
					T1."InsTotal" - T1."PaidToDate" 
					- T0."WTSum"
			END))AS DECIMAL(16,2))
			AS "Montototal",
			CAST(
			ifnull((SELECT MAX(R0."UsrNumber3") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCur"and "U_EXC_ACTIVO" = 'Y' ),'0')
			AS BIGINT)
			AS "Cuenta"
		FROM 
			ODPO T0
			LEFT JOIN DPO6 T1 ON T0."DocEntry" = T1."DocEntry" AND T1."InstlmntID" = '1'
			LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
			LEFT JOIN "@EXP_PMP1" T4
			ON T0."DocEntry" = T4."U_EXP_DOCENTRYDOC"
			AND T1."DocEntry" = T4."U_EXP_DOCENTRYDOC"
			--AND T4."U_SMC_TIPO_DOCUMENTO" = 'SA-P'
		WHERE
			T0."Indicator" IN ('00','01','02','14','50','99','05','DI')
			AND T1."InstlmntID" = '1'
			AND T0."CreateTran" = 'N'
			AND T2."PymntGroup" not like '%DT%'
			AND IFNULL(T4."U_EXP_SLC_PAGO",'N') = 'Y'
			AND IFNULL(T4."U_EXP_SLC_RETENCION",'N') = 'Y'
			AND T4."DocEntry"= :DocEntryPM
			--AND T4."U_SMC_TIPO_DOCUMENTO" = 'SA-P'
			--and T0."DocTotal" = T4."U_SMC_MONTO"
			AND T0."DocStatus" != 'C'
			
			
			
			union
			
			
			SELECT
			/*
			CAST(IFNULL(T4."U_EXP_IMPORTE",(CASE 
				WHEN T0."DocCur" in ('USD','EUR') 
					THEN T1."InsTotalFC" - T1."PaidFC"
					- T0."WTSumFC"
				ELSE 
					T1."InsTotal" - T1."PaidToDate" 
					- T0."WTSum"
			END))AS DECIMAL(16,2))
			AS "Montototal",*/
			CAST( 
			(CASE 
				WHEN T0."DocCurr" in ('USD','EUR') 
					THEN T0."NoDocSumFC" 
				ELSE 
					T0."NoDocSum"
			END)AS DECIMAL(16,2)) AS "Montototal",
			
			CAST(
			ifnull((SELECT MAX(R0."UsrNumber3") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND IFNULL(R0."UsrNumber1",'SOL') = T0."DocCurr"and "U_EXC_ACTIVO" = 'Y' ),'0')
			AS BIGINT)
			AS "Cuenta"
		FROM 
			OPDF T0
			--LEFT JOIN DPO6 T1 ON T0."DocEntry" = T1."DocEntry" AND T1."InstlmntID" = '1'
			--LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
			LEFT JOIN "@EXP_PMP1" T4
			ON T0."DocEntry" = T4."U_EXP_DOCENTRYDOC"
			--AND T1."DocEntry" = T4."U_EXP_DOCENTRYDOC"
			--AND T4."U_SMC_TIPO_DOCUMENTO" = 'SP'
		WHERE
			--T0."Indicator" IN ('01','02','14','50','99','05','DI')
			--AND T1."InstlmntID" = '1'
			--AND T0."CreateTran" = 'N'
			--AND T2."PymntGroup" not like '%DT%'
			IFNULL(T4."U_EXP_SLC_PAGO",'N') = 'Y'
			AND IFNULL(T4."U_EXP_SLC_RETENCION",'N') = 'Y'
			AND T4."DocEntry"= :DocEntryPM
			--AND T4."U_SMC_TIPO_DOCUMENTO" = 'SP'
			--and T0."DocTotal" = T4."U_SMC_MONTO"
			
	
			
			
				
			union
			
			
			SELECT
			distinct
			CAST( 
		(CASE 
			WHEN ifnull(T1."FCCurrency",'SOL') in ('USD','EUR') 
				THEN (T0."FcTotal" - ifnull((select sum(TT0."ReconSumFC") from 
				"ITR1" TT0 where TT0."TransId" = T0."TransId"
				GROUP BY TT0."TransId"),0))
			ELSE 
				(T0."LocTotal" - ifnull((select sum(TT0."ReconSum") from 
				"ITR1" TT0 where TT0."TransId" = T0."TransId"
				GROUP BY TT0."TransId"),0))
		END)AS DECIMAL(16,2)) AS "Montototal",
			
			CAST(
			ifnull((SELECT MAX(R0."UsrNumber3") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '002' AND IFNULL(R0."UsrNumber1",'SOL') = T1."FCCurrency"and "U_EXC_ACTIVO" = 'Y' ),'0')
			AS BIGINT)
			AS "Cuenta"
		FROM 
			OJDT T0
			LEFT JOIN JDT1 T1 ON T0."TransId" = T1."TransId"
			--LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
			inner JOIN OCRD T3 ON T1."ShortName" = T3."CardCode"
			LEFT JOIN "@EXP_PMP1" T4
			ON T0."TransId" = T4."U_EXP_DOCENTRYDOC"
			--AND T1."DocEntry" = T4."U_EXP_DOCENTRYDOC"
			--AND T4."U_SMC_TIPO_DOCUMENTO" = 'AS'
		WHERE
			--T0."Indicator" IN ('01','02','14','50','99','05','DI')
			--AND T1."InstlmntID" = '1'
			--AND T0."CreateTran" = 'N'
			--AND T2."PymntGroup" not like '%DT%'
			IFNULL(T4."U_EXP_SLC_PAGO",'N') = 'Y'
			AND IFNULL(T4."U_EXP_SLC_RETENCION",'N') = 'Y'
			AND T4."DocEntry"= :DocEntryPM
			--AND T4."U_SMC_TIPO_DOCUMENTO" = 'AS'
			--and T0."DocTotal" = T4."U_SMC_MONTO"
			and (CASE 
			WHEN ifnull(T1."FCCurrency",'SOL') in ('USD','EUR') 
				THEN (T0."FcTotal" - ifnull((select sum(TT0."ReconSumFC") from 
				"ITR1" TT0 where TT0."TransId" = T0."TransId"
				GROUP BY TT0."TransId"),0))
			ELSE 
				(T0."LocTotal" - ifnull((select sum(TT0."ReconSum") from 
				"ITR1" TT0 where TT0."TransId" = T0."TransId"
				GROUP BY TT0."TransId"),0))
		END) >0
			
			
			
			union
			
			
			
			
			
			
			
			SELECT
			distinct
			CAST( 
		(CASE 
			WHEN ifnull(T1."FCCurrency",'SOL') in ('USD','EUR') 
			then
				(T0."FcTotal" - ifnull((select sum(TT0."ReconSumFC") from 
				"ITR1" TT0 where TT0."TransId" = T0."TransId"
				GROUP BY TT0."TransId"),0)) 
			ELSE 
				(T0."LocTotal" - ifnull((select sum(TT0."ReconSum") from 
				"ITR1" TT0 where TT0."TransId" = T0."TransId"
				GROUP BY TT0."TransId"),0))
		END)AS DECIMAL(16,2)) AS "Montototal",
			
			CAST(
			ifnull((SELECT MAX(R0."UsrNumber3") FROM OCRB R0 WHERE R0."CardCode" = T3."CardCode" AND "BankCode" = '002' AND IFNULL(R0."UsrNumber1",'SOL') = T1."FCCurrency"and "U_EXC_ACTIVO" = 'Y' ),'0')
			AS BIGINT)
			AS "Cuenta"
		FROM 
			OJDT T0
			LEFT JOIN JDT1 T1 ON T0."TransId" = T1."TransId"
			--LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
			inner JOIN OCRD T3 ON T1."ShortName" = T3."CardCode"
			LEFT JOIN "@EXP_PMP1" T4
			ON T0."TransId" = T4."U_EXP_DOCENTRYDOC"
			--AND T1."DocEntry" = T4."U_EXP_DOCENTRYDOC"
			--AND T4."U_SMC_TIPO_DOCUMENTO" = 'PR'
		WHERE
			--T0."Indicator" IN ('01','02','14','50','99','05','DI')
			--AND T1."InstlmntID" = '1'
			--AND T0."CreateTran" = 'N'
			--AND T2."PymntGroup" not like '%DT%'
			IFNULL(T4."U_EXP_SLC_PAGO",'N') = 'Y'
			AND IFNULL(T4."U_EXP_SLC_RETENCION",'N') = 'Y'
			AND T4."DocEntry"= :DocEntryPM
			AND T1."Credit" > 0
			--AND T4."U_SMC_TIPO_DOCUMENTO" = 'PR'
			--and T0."DocTotal" = T4."U_SMC_MONTO"
			and (CASE 
			WHEN ifnull(T1."FCCurrency",'SOL') in ('USD','EUR') 
			then
				(T0."FcTotal" - ifnull((select sum(TT0."ReconSumFC") from 
				"ITR1" TT0 where TT0."TransId" = T0."TransId"
				GROUP BY TT0."TransId"),0)) 
			ELSE 
				(T0."LocTotal" - ifnull((select sum(TT0."ReconSum") from 
				"ITR1" TT0 where TT0."TransId" = T0."TransId"
				GROUP BY TT0."TransId"),0))
		END)> 0
			
			
			
			
			
			) Z0 --where Z0."Cuenta" > 0
		);
	
	CREATE LOCAL TEMPORARY TABLE  "#tmpTotalSum" AS 
	(
		select * from :lt_data_tmp_1
	);
	
	
	select * from (
	
	SELECT
		TOP 1
		'1' AS "TipoRegistro",
		
		LPAD(CAST(ROW_NUMBER() OVER (ORDER BY T1."U_EXP_DOCENTRYDOC" DESC) AS VARCHAR)
		,6,'0')
		AS "CantidadAbonos",
		TO_VARCHAR(T0."U_EXP_FECHAPAGO",'DDMMYYYY') AS "FechaProceso",
		
		(SELECT ifnull("UsrNumber2",'C') FROM DSC1 WHERE "GLAccount" = T1."U_EXP_CODCTABANCO") AS "TipoCuenta", --REPLACE(T0."U_SMC_CTACONT",'-',''))) AS "TipoCuenta",
		
		CASE
			WHEN T2."DocCur" = 'SOL' THEN 'S/'
			ELSE 'US' 
		END AS "Moneda",
		RPAD(REPLACE(
			(SELECT "Account" FROM DSC1 WHERE "GLAccount" = T1."U_EXP_CODCTABANCO")
		,'-','')
		,20) AS "CuentaCargo",
		LPAD(CAST(
			(SELECT MAX(R0."MontototalSum") FROM "#tmpTotalSum" R0)
		AS VARCHAR),16,'0') 
		AS "Montototal",
		RPAD('Pago a Proveedores',40) AS "Referencia",
		'N' AS "Validacion",
		LPAD(CAST((
			(SELECT MAX(R0."TotalCuenta") FROM "#tmpTotalSum" R0)
			+
			/*
			CAST(
			(SUBSTRING(
				(REPLACE(
					(SELECT "Account" FROM DSC1 WHERE "GLAccount" = 
					(SELECT "AcctCode" FROM OACT WHERE "FormatCode" = REPLACE(T0."U_SMC_CTACONT",'-','')))
				 ,'-',''))
				,4,
				(LENGTH((REPLACE(
					(SELECT "Account" FROM DSC1 WHERE "GLAccount" = 
					(SELECT "AcctCode" FROM OACT WHERE "FormatCode" = REPLACE(T0."U_SMC_CTACONT",'-','')))
				 ,'-','')))-3))
			) AS BIGINT)
			*/
			CAST((SELECT "Account" FROM DSC1 WHERE "GLAccount" = T1."U_EXP_CODCTABANCO") AS BIGINT)
					
			)AS VARCHAR),15,'0')
		AS "Cadena"
	FROM 
		"@EXP_OPMP" T0
		LEFT JOIN "@EXP_PMP1" T1 ON T0."DocEntry" = T1."DocEntry" --AND T1."U_SMC_TIPO_DOCUMENTO" = 'FT-P'
		LEFT JOIN OPCH T2 ON T2."DocEntry" = T1."U_EXP_DOCENTRYDOC" AND T1."U_EXP_TIPODOC" = T2."ObjType"
	WHERE
		T0."DocEntry" = :DocEntryPM
		--AND T1."U_EXP_TIPODOC" = T0."U_EXP_TIPODOC"
		--T2."DocTotal" = T1."U_SMC_MONTO"
		
	--nuevo	
	union

		
	SELECT
		TOP 1
		'1' AS "TipoRegistro",
		LPAD(CAST(ROW_NUMBER() OVER (ORDER BY T1."U_EXP_DOCENTRYDOC" DESC) AS VARCHAR)
		,6,'0')
		AS "CantidadAbonos",
		TO_VARCHAR(T0."U_EXP_FECHAPAGO",'DDMMYYYY') AS "FechaProceso",
		(SELECT ifnull("UsrNumber2",'C') FROM DSC1 WHERE "GLAccount" =  T1."U_EXP_CODCTABANCO") AS "TipoCuenta",
		CASE
			WHEN T2."DocCur" = 'SOL' THEN 'S/'
			ELSE 'US' 
		END AS "Moneda",
		RPAD(REPLACE(
			(SELECT "Account" FROM DSC1 WHERE "GLAccount" = T1."U_EXP_CODCTABANCO")
		,'-','')
		,20) AS "CuentaCargo",
		LPAD(CAST(
			(SELECT MAX(R0."MontototalSum") FROM "#tmpTotalSum" R0)
		AS VARCHAR),16,'0') 
		AS "Montototal",
		RPAD('Pago a Proveedores',40) AS "Referencia",
		'N' AS "Validacion",
		LPAD(CAST((
			(SELECT MAX(R0."TotalCuenta") FROM "#tmpTotalSum" R0)
			+
			/*
			CAST(
			(	SUBSTRING(
				(REPLACE(
					(SELECT "Account" FROM DSC1 WHERE "GLAccount" = 
					(SELECT "AcctCode" FROM OACT WHERE "FormatCode" = REPLACE(T0."U_SMC_CTACONT",'-','')))
				 ,'-',''))
				,4,
				(LENGTH((REPLACE(
					(SELECT "Account" FROM DSC1 WHERE "GLAccount" = 
					(SELECT "AcctCode" FROM OACT WHERE "FormatCode" = REPLACE(T0."U_SMC_CTACONT",'-','')))
				 ,'-','')))-3))
			) AS BIGINT)
*/
				CAST((SELECT "Account" FROM DSC1 WHERE "GLAccount" = T1."U_EXP_CODCTABANCO") AS BIGINT)
			)AS VARCHAR),15,'0')
		AS "Cadena"
	FROM 
		"@EXP_OPMP" T0
		LEFT JOIN "@EXP_PMP1" T1 ON T0."DocEntry" = T1."DocEntry" --AND T1."U_EXP_TIPODOC" = 'NC-C'
		LEFT JOIN ORIN T2 ON T2."DocEntry" = T1."U_EXP_DOCENTRYDOC" AND T1."U_EXP_TIPODOC" = T2."ObjType"
	WHERE
		T0."DocEntry" = :DocEntryPM
		--AND T1."U_SMC_TIPO_DOCUMENTO" = 'NC-C'
		--T2."DocTotal" = T1."U_SMC_MONTO"
		
	union
		
		
	SELECT
		TOP 1
		'1' AS "TipoRegistro",
		LPAD(CAST(ROW_NUMBER() OVER (ORDER BY T1."U_EXP_DOCENTRYDOC" DESC) AS VARCHAR)
		,6,'0')
		AS "CantidadAbonos",
		TO_VARCHAR(T0."U_EXP_FECHAPAGO",'DDMMYYYY') AS "FechaProceso",
		(SELECT ifnull("UsrNumber2",'C') FROM DSC1 WHERE "GLAccount" = T1."U_EXP_CODCTABANCO") AS "TipoCuenta",
		CASE
			WHEN T2."DocCur" = 'SOL' THEN 'S/'
			ELSE 'US' 
		END AS "Moneda",
		RPAD(REPLACE(
			(SELECT "Account" FROM DSC1 WHERE "GLAccount" = T1."U_EXP_CODCTABANCO")
		,'-','')
		,20) AS "CuentaCargo",
		LPAD(CAST(
			(SELECT MAX(R0."MontototalSum") FROM "#tmpTotalSum" R0)
		AS VARCHAR),16,'0') 
		AS "Montototal",
		RPAD('Pago a Proveedores',40) AS "Referencia",
		'N' AS "Validacion",
		LPAD(CAST((
			(SELECT MAX(R0."TotalCuenta") FROM "#tmpTotalSum" R0)
			+
			/*
			CAST(
			(	SUBSTRING(
				(REPLACE(
					(SELECT "Account" FROM DSC1 WHERE "GLAccount" = 
					(SELECT "AcctCode" FROM OACT WHERE "FormatCode" = REPLACE(T0."U_SMC_CTACONT",'-','')))
				 ,'-',''))
				,4,
				(LENGTH((REPLACE(
					(SELECT "Account" FROM DSC1 WHERE "GLAccount" = 
					(SELECT "AcctCode" FROM OACT WHERE "FormatCode" = REPLACE(T0."U_SMC_CTACONT",'-','')))
				 ,'-','')))-3))
			) AS BIGINT)*/
			CAST((SELECT "Account" FROM DSC1 WHERE "GLAccount" = T1."U_EXP_CODCTABANCO") AS BIGINT)
			)AS VARCHAR),15,'0')
		AS "Cadena"
	FROM 
		"@EXP_OPMP" T0
		LEFT JOIN "@EXP_PMP1" T1 ON T0."DocEntry" = T1."DocEntry" --AND T1."U_SMC_TIPO_DOCUMENTO" = 'FA-P'
		LEFT JOIN ODPO T2 ON T2."DocEntry" = T1."U_EXP_DOCENTRYDOC" AND T1."U_EXP_TIPODOC" = T2."ObjType"
	WHERE
		T0."DocEntry" = :DocEntryPM and 
		T2."CreateTran" = 'Y'
		--AND T1."U_SMC_TIPO_DOCUMENTO" = 'FA-P'
		--T2."DocTotal" = T1."U_SMC_MONTO"


	union
		
		
	SELECT
		TOP 1
		'1' AS "TipoRegistro",
		LPAD(CAST(ROW_NUMBER() OVER (ORDER BY T1."U_EXP_DOCENTRYDOC" DESC) AS VARCHAR)
		,6,'0')
		AS "CantidadAbonos",
		TO_VARCHAR(T0."U_EXP_FECHAPAGO",'DDMMYYYY') AS "FechaProceso",
		(SELECT ifnull("UsrNumber2",'C') FROM DSC1 WHERE "GLAccount" = T1."U_EXP_CODCTABANCO") AS "TipoCuenta",
		CASE
			WHEN T2."DocCur" = 'SOL' THEN 'S/'
			ELSE 'US' 
		END AS "Moneda",
		RPAD(REPLACE(
			(SELECT "Account" FROM DSC1 WHERE "GLAccount" = T1."U_EXP_CODCTABANCO"),'-','')
		,20) AS "CuentaCargo",
		LPAD(CAST(
			(SELECT MAX(R0."MontototalSum") FROM "#tmpTotalSum" R0)
		AS VARCHAR),16,'0') 
		AS "Montototal",
		RPAD('Pago a Proveedores',40) AS "Referencia",
		'N' AS "Validacion",
		LPAD(CAST((
			(SELECT MAX(R0."TotalCuenta") FROM "#tmpTotalSum" R0)
			+
			/*
			CAST(
			(	SUBSTRING(
				(REPLACE(
					(SELECT "Account" FROM DSC1 WHERE "GLAccount" = 
					(SELECT "AcctCode" FROM OACT WHERE "FormatCode" = REPLACE(T0."U_SMC_CTACONT",'-','')))
				 ,'-',''))
				,4,
				(LENGTH((REPLACE(
					(SELECT "Account" FROM DSC1 WHERE "GLAccount" = 
					(SELECT "AcctCode" FROM OACT WHERE "FormatCode" = REPLACE(T0."U_SMC_CTACONT",'-','')))
				 ,'-','')))-3))
			) AS BIGINT)
*/
			CAST((SELECT "Account" FROM DSC1 WHERE "GLAccount" = T1."U_EXP_CODCTABANCO") AS BIGINT)
			)AS VARCHAR),15,'0')
		AS "Cadena"
	FROM 
		"@EXP_OPMP" T0
		LEFT JOIN "@EXP_PMP1" T1 ON T0."DocEntry" = T1."DocEntry" --AND T1."U_SMC_TIPO_DOCUMENTO" = 'SA-P'
		LEFT JOIN ODPO T2 ON T2."DocEntry" = T1."U_EXP_DOCENTRYDOC" AND T1."U_EXP_TIPODOC" = T2."ObjType"
	WHERE
		T0."DocEntry" = :DocEntryPM and 
		T2."CreateTran" = 'N'
		--AND T1."U_SMC_TIPO_DOCUMENTO" = 'SA-P'
		--T2."DocTotal" = T1."U_SMC_MONTO"

	union
		
	SELECT
		TOP 1
		'1' AS "TipoRegistro",
		LPAD(CAST(ROW_NUMBER() OVER (ORDER BY T1."U_EXP_DOCENTRYDOC" DESC) AS VARCHAR)
		,6,'0')
		AS "CantidadAbonos",
		TO_VARCHAR(T0."U_EXP_FECHAPAGO",'DDMMYYYY') AS "FechaProceso",
		(SELECT ifnull("UsrNumber2",'C') FROM DSC1 WHERE "GLAccount" = 
				T1."U_EXP_CODCTABANCO") AS "TipoCuenta",
		CASE
			WHEN T2."DocCurr" = 'SOL' THEN 'S/'
			ELSE 'US' 
		END AS "Moneda",
		RPAD(REPLACE(
			(SELECT "Account" FROM DSC1 WHERE "GLAccount" = 
				T1."U_EXP_CODCTABANCO"
		),'-','')
		,20) AS "CuentaCargo",
		LPAD(CAST(
			(SELECT MAX(R0."MontototalSum") FROM "#tmpTotalSum" R0)
		AS VARCHAR),16,'0') 
		AS "Montototal",
		RPAD('Pago a Proveedores',40) AS "Referencia",
		'N' AS "Validacion",
		LPAD(CAST((
			(SELECT MAX(R0."TotalCuenta") FROM "#tmpTotalSum" R0)
			+

			CAST((SELECT "Account" FROM DSC1 WHERE "GLAccount" = T1."U_EXP_CODCTABANCO") AS BIGINT)
			)AS VARCHAR),15,'0')
		AS "Cadena"
	FROM 
		"@EXP_OPMP" T0
		LEFT JOIN "@EXP_PMP1" T1 ON T0."DocEntry" = T1."DocEntry" --AND T1."U_SMC_TIPO_DOCUMENTO" = 'SP'
		LEFT JOIN OPDF T2 ON T2."DocEntry" = T1."U_EXP_DOCENTRYDOC" AND T1."U_EXP_TIPODOC" = T2."ObjType"
	WHERE
		T0."DocEntry" = :DocEntryPM --and 
		--T2."CreateTran" = 'N'
		--AND T1."U_SMC_TIPO_DOCUMENTO" = 'SP'
		--T2."DocTotal" = T1."U_SMC_MONTO"

	union
	
	SELECT
		TOP 1
		'1' AS "TipoRegistro",
		LPAD(CAST(ROW_NUMBER() OVER (ORDER BY T1."U_EXP_DOCENTRYDOC" DESC) AS VARCHAR)
		,6,'0')
		AS "CantidadAbonos",
		TO_VARCHAR(T0."U_EXP_FECHAPAGO",'DDMMYYYY') AS "FechaProceso",
		(SELECT ifnull("UsrNumber2",'C') FROM DSC1 WHERE "GLAccount" = 
				T1."U_EXP_CODCTABANCO") AS "TipoCuenta",
		/*CASE
			WHEN ifnull(T2."TransCurr",'SOL') = 'SOL' THEN 'S/'
			ELSE 'US' 
		END AS "Moneda",*/
		(CASE
			WHEN LENGTH(T2."TransCurr") > 0 THEN (CASE WHEN T2."TransCurr" = 'SOL' THEN 'S/' ELSE 'US' END)
			ELSE 'S/' 
		END) AS "Moneda",

		RPAD(REPLACE(
			(SELECT "Account" FROM DSC1 WHERE "GLAccount" = 
				T1."U_EXP_CODCTABANCO"
		),'-','')
		,20) AS "CuentaCargo",
		LPAD(CAST(
			(SELECT MAX(R0."MontototalSum") FROM "#tmpTotalSum" R0)
		AS VARCHAR),16,'0') 
		AS "Montototal",
		RPAD('Pago a Proveedores',40) AS "Referencia",
		'N' AS "Validacion",
		LPAD(CAST((
			(SELECT MAX(R0."TotalCuenta") FROM "#tmpTotalSum" R0)
			+

			CAST((SELECT "Account" FROM DSC1 WHERE "GLAccount" = 
					T1."U_EXP_CODCTABANCO") AS BIGINT)
			)AS VARCHAR),15,'0')
		AS "Cadena"
	FROM 
		"@EXP_OPMP" T0
		LEFT JOIN "@EXP_PMP1" T1 ON T0."DocEntry" = T1."DocEntry" --AND T1."U_SMC_TIPO_DOCUMENTO" = 'AS'
		LEFT JOIN OJDT T2 ON T2."TransId" = T1."U_EXP_DOCENTRYDOC" AND T1."U_EXP_TIPODOC" = T2."ObjType"
	WHERE
		T0."DocEntry" = :DocEntryPM --and 
		--T2."CreateTran" = 'N'
		--AND T1."U_SMC_TIPO_DOCUMENTO" = 'AS'
		--T2."DocTotal" = T1."U_SMC_MONTO"

	union
			
	SELECT
		TOP 1
		'1' AS "TipoRegistro",
		LPAD(CAST(ROW_NUMBER() OVER (ORDER BY T1."U_EXP_DOCENTRYDOC" DESC) AS VARCHAR)
		,6,'0')
		AS "CantidadAbonos",
		TO_VARCHAR(T0."U_EXP_FECHAPAGO",'DDMMYYYY') AS "FechaProceso",
		(SELECT ifnull("UsrNumber2",'C') FROM DSC1 WHERE "GLAccount" = 
				T1."U_EXP_CODCTABANCO") AS "TipoCuenta",
		/*CASE
			WHEN ifnull(T2."TransCurr",'SOL') = 'SOL' THEN 'S/'
			ELSE 'US' 
		END AS "Moneda",*/
		(CASE
			WHEN LENGTH(T2."TransCurr") > 0 THEN (CASE WHEN T2."TransCurr" = 'SOL' THEN 'S/' ELSE 'US' END)
			ELSE 'S/' 
		END) AS "Moneda",
		
		RPAD(REPLACE(
			(SELECT "Account" FROM DSC1 WHERE "GLAccount" = 
				T1."U_EXP_CODCTABANCO"
		),'-','')
		,20) AS "CuentaCargo",
		LPAD(CAST(
			(SELECT MAX(R0."MontototalSum") FROM "#tmpTotalSum" R0)
		AS VARCHAR),16,'0') 
		AS "Montototal",
		RPAD('Pago a Proveedores',40) AS "Referencia",
		'N' AS "Validacion",
		LPAD(CAST((
			(SELECT MAX(R0."TotalCuenta") FROM "#tmpTotalSum" R0)
			+
			
CAST((SELECT "Account" FROM DSC1 WHERE "GLAccount" = 
					T1."U_EXP_CODCTABANCO") AS BIGINT)
			)AS VARCHAR),15,'0')
		AS "Cadena"
	FROM 
		"@EXP_OPMP" T0
		LEFT JOIN "@EXP_PMP1" T1 ON T0."DocEntry" = T1."DocEntry" --AND T1."U_SMC_TIPO_DOCUMENTO" = 'PR'
		LEFT JOIN OJDT T2 ON T2."TransId" = T1."U_EXP_DOCENTRYDOC" AND T1."U_EXP_TIPODOC" = '24'
	WHERE
		T0."DocEntry" = :DocEntryPM --and 
		--T2."CreateTran" = 'N'
		--AND T1."U_SMC_TIPO_DOCUMENTO" = 'PR'
		--T2."DocTotal" = T1."U_SMC_MONTO"




	) Z0

	WHERE "Moneda" = (CASE WHEN :Moneda = 'SOL' THEN 'S/' ELSE 'US' END	)
		
	ORDER BY
		2
	DESC;
	
	DROP TABLE "#tmpTotalSum";
	
END