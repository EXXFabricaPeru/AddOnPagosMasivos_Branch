CREATE PROCEDURE SMC_TERCERORETENEDOR
(
	IN docEntry INT
)
AS 
BEGIN

SELECT
F."RUC",
SUM(F."Monto") AS "Monto",
STRING_AGG(F."Documentos",'|') AS "Documentos" FROM (
SELECT
	Z0."RUC",
	SUM(Z0."Monto") as "Monto",
	STRING_AGG(Z0."Documento",'|') as "Documentos"
FROM
	(
	---- LOTE 1 DE DOCUMENTOS:

	----------------- FACTURAS DE PROVEEDORES --------------
	SELECT
		T2."ObjType" ||'-'||T2."DocEntry" as "Documento",
		T3."LicTradNum" as "RUC",
		CAST(ROUND(
				(CASE T2."DocCur"
					WHEN 'USD' THEN	T1."U_EXP_IMPORTE"*T5."Rate"
					WHEN 'SOL' THEN T1."U_EXP_IMPORTE"
				  END)
				,2)
		AS DECIMAL(10,2)) as "Monto" 
	FROM 
	"@EXP_OPMP" T0
	INNER JOIN "@EXP_PMP1" 	T1 ON T0."DocEntry" = T1."DocEntry"
	INNER JOIN "OPCH" 		T2 ON T2."DocEntry"=T1."U_EXP_DOCENTRYDOC" AND T1."U_EXP_TIPODOC" = T2."ObjType"
	INNER JOIN "OCRD" 		T3 ON T3."CardCode"=T2."CardCode"
	INNER JOIN "ORTT" 		T5 ON T5."RateDate"=T0."U_EXP_FECHA" AND T5."Currency"='USD'  
	WHERE 
	IFNULL(T2."U_EXX_NUMEREND",'')='' -- No considera documentos de caja chica ni rendiciones
	AND T2."Indicator" NOT IN ('00','14','02','DI','AN','SA') -- No considera los recibos publicos
	AND T0."DocEntry"=:docEntry
	--AND ifnull(T1."U_EXP_SLC_PAGO",'') = 'Y'
	--AND T1."U_SMC_TIPO_DOCUMENTO"='FT-P'
	AND T3."U_EXX_TIPODOCU"='6'
		
	UNION
	
	----------------- NOTA DE CREDITO DE CLIENTES --------------
	SELECT
		T2."ObjType" ||'-'||T2."DocEntry" as "Documento",
		T3."LicTradNum" as "RUC",
		CAST(ROUND(
				(CASE T2."DocCur"
					WHEN 'USD' THEN	T1."U_EXP_IMPORTE"*T5."Rate"
					WHEN 'SOL' THEN T1."U_EXP_IMPORTE"
				  END)
				,2)
		AS DECIMAL(10,2)) as "Monto"
	FROM 
	"@EXP_OPMP" T0
	INNER JOIN "@EXP_PMP1" 			T1 ON T1."DocEntry"=T0."DocEntry"
	INNER JOIN "ORIN" 				T2 ON T2."DocEntry"=T1."U_EXP_DOCENTRYDOC" AND T1."U_EXP_TIPODOC" = T2."ObjType"
	INNER JOIN "OCRD" 				T3 ON T3."CardCode"=T2."CardCode"
	LEFT JOIN "@SMC_APM_3RETCONF" 	T4 ON T4."U_SMC_TIPODOC" = T2."Indicator"
	INNER JOIN "ORTT" T5 ON T5."RateDate"=T0."U_EXP_FECHA" AND T5."Currency"='USD' 
	WHERE 
	IFNULL(T2."U_EXX_NUMEREND",'')='' -- No considera documentos de caja chica ni rendiciones
	AND T2."Indicator" NOT IN ('00','14') -- No considera los recibos publicos
	AND T0."DocEntry"=:docEntry
	--AND ifnull(T1."U_EXP_SLC_PAGO",'') = 'Y'
	--AND T1."U_SMC_TIPO_DOCUMENTO"='NC-C'
	AND T3."U_EXX_TIPODOCU"='6'
	
	UNION
	
	----------------- SOLICITUD DE ANTICIPO - FACTURA DE ANTICIPO --------------
	SELECT
		T2."ObjType" ||'-'||T2."DocEntry" as "Documento",
		T3."LicTradNum" as "RUC",
		CAST(ROUND(
				(CASE T2."DocCur"
					WHEN 'USD' THEN	T1."U_EXP_IMPORTE"*T5."Rate"
					WHEN 'SOL' THEN T1."U_EXP_IMPORTE"
				  END)
				,2)
		AS DECIMAL(10,2)) as "Monto"
	FROM 
	"@EXP_OPMP" T0
	INNER JOIN "@EXP_PMP1" 			T1 ON T1."DocEntry"=T0."DocEntry"
	INNER JOIN "ODPO" 				T2 ON T2."DocEntry"=T1."U_EXP_DOCENTRYDOC" AND T1."U_EXP_TIPODOC" = T2."ObjType"
	INNER JOIN "OCRD" 				T3 ON T3."CardCode"=T2."CardCode"
	LEFT JOIN "@SMC_APM_3RETCONF" 	T4 ON T4."U_SMC_TIPODOC" = T2."Indicator"
	INNER JOIN "ORTT" T5 ON T5."RateDate"=T0."U_EXP_FECHA" AND T5."Currency"='USD' 
	WHERE 
	IFNULL(T2."U_EXX_NUMEREND",'')='' -- No considera documentos de caja chica ni rendiciones
	AND T2."Indicator" NOT IN ('00','14','SA') -- No considera los recibos publicos
	AND T0."DocEntry"=:docEntry
	--AND ifnull(T1."U_EXP_SLC_PAGO",'') = 'Y'
	--AND T1."U_SMC_TIPO_DOCUMENTO" IN ('SA-P','FA-P')
	AND T3."U_EXX_TIPODOCU"='6'
	/*	
	UNION
	
	----------------- PAGOS EFECTUADOS PRELIMINARES --------------
	SELECT
		T3."LicTradNum" as "RUC",
		CAST(ROUND(
				(CASE T2."DocCurr"
					WHEN 'USD' THEN	T1."U_EXP_IMPORTE"*T5."Rate"
					WHEN 'SOL' THEN T1."U_EXP_IMPORTE"
				  END)
				,2)
		AS DECIMAL(10,2)) as "Monto"
	FROM 
	"@EXP_OPMP" T0
	INNER JOIN "@EXP_PMP1" T1 	ON T1."DocEntry"=T0."DocEntry"
	INNER JOIN "OPDF" T2 		ON T2."DocEntry"=T1."U_EXP_DOCENTRYDOC" AND T1."U_EXP_TIPODOC" = T2."ObjType"
	INNER JOIN "OCRD" T3 		ON T3."CardCode"=T2."CardCode"
	INNER JOIN "ORTT" T5 		ON T5."RateDate"=T0."U_EXP_FECHA" AND T5."Currency"='USD' 
	WHERE 
	T0."DocEntry"=:docEntry
	--AND T1."U_SMC_TIPO_DOCUMENTO"='SP'	
		*/
	UNION
	
	----------------- ASIENTOS --------------
	SELECT
		T2."ObjType" ||'-'||T2."TransId" as "Documento",
		T4."LicTradNum" as "RUC",
		CAST(ROUND(
				(CASE T3."FCCurrency"
					WHEN 'USD' THEN	T1."U_EXP_IMPORTE"*T5."Rate"
					ELSE T1."U_EXP_IMPORTE"
				  END)
				,2)
		AS DECIMAL(10,2)) as "Monto"
	FROM 
	"@EXP_OPMP" T0
	INNER JOIN "@EXP_PMP1" T1 	ON T1."DocEntry"=T0."DocEntry"
	INNER JOIN "OJDT" T2 		ON T2."TransId"=T1."U_EXP_DOCENTRYDOC" AND T1."U_EXP_TIPODOC" = T2."ObjType"
	INNER JOIN "JDT1" T3 		ON T3."TransId"=T2."TransId"
	INNER JOIN "OCRD" T4 		ON T4."CardCode"=T3."ShortName"
	INNER JOIN "ORTT" T5 		ON T5."RateDate"=T0."U_EXP_FECHA" AND T5."Currency"='USD' 
	WHERE 
	T0."DocEntry"=:docEntry
	--AND ifnull(T1."U_EXP_SLC_PAGO",'') = 'Y'
	--AND T1."U_SMC_TIPO_DOCUMENTO"='AS'	
	AND T4."U_EXX_TIPODOCU"='6' AND T1."U_EXP_ASNROLINEA"=T3."Line_ID"
	
	UNION
	
	----------------- PAGOS RECIBIDOS --------------
	SELECT
		T2."ObjType" ||'-'||T2."DocEntry" as "Documento",
		T3."LicTradNum" as "RUC",
		CAST(ROUND(
				(CASE T2."DocCurr"
					WHEN 'USD' THEN	T1."U_EXP_IMPORTE"*T5."Rate"
					WHEN 'SOL' THEN T1."U_EXP_IMPORTE"
				  END)
				,2)
		AS DECIMAL(10,2)) as "Monto"
	FROM 
	"@EXP_OPMP" T0
	INNER JOIN "@EXP_PMP1" 	T1 ON T1."DocEntry"=T0."DocEntry"
	INNER JOIN "ORCT" 		T2 ON T2."DocEntry"=T1."U_EXP_DOCENTRYDOC" AND T1."U_EXP_TIPODOC" = T2."ObjType"
	INNER JOIN "OCRD" 		T3 ON T3."CardCode"=T2."CardCode"
	INNER JOIN "ORTT" 		T5 ON T5."RateDate"=T0."U_EXP_FECHA" AND T5."Currency"='USD' 
	WHERE 
	T0."DocEntry"=:docEntry
	--AND ifnull(T1."U_EXP_SLC_PAGO",'') = 'Y'
		--AND T1."U_SMC_TIPO_DOCUMENTO"='SP'	
	AND T3."U_EXX_TIPODOCU"='6'
		
	)Z0
	, "@EXP_OPMP" T10
	INNER JOIN "ORTT" T5 ON YEAR(T5."RateDate")=YEAR(T10."U_EXP_FECHA") AND T5."Currency"='UIT' AND MONTH(T5."RateDate")=1
	WHERE T10."DocEntry"=:docEntry
	GROUP BY 
	Z0."RUC"
	HAVING SUM(Z0."Monto")>MAX(T5."Rate")
	
	UNION ALL
	
	
	---- LOTE 2 DE DOCUMENTOS:
	
	----------------- FACTURAS DE PROVEEDORES - RECIBOS POR HONORARIOS--------------
	SELECT
	RH."RUC",
	CAST(ROUND(((SUM(RH."Monto")
		-(SELECT "U_SMC_UNDRP"*"U_SMC_REFPRO"*(SELECT MAX("Rate") FROM ORTT WHERE "Currency"='UIT' AND YEAR("RateDate")=YEAR((SELECT "U_EXP_FECHA" FROM "@EXP_OPMP" WHERE "DocEntry"=:docEntry))) FROM "@SMC_APM_3RETCONF" WHERE "U_SMC_TIPODOC"='02'))
		/3),2) AS DECIMAL(10,2)) AS "Monto",
	STRING_AGG(RH."Documento",'|') AS "Documentos" FROM (
	SELECT
		T2."ObjType" ||'-'||T2."DocEntry" as "Documento",
		T3."LicTradNum" as "RUC",
		CAST(ROUND(
				(CASE T2."DocCur"
					WHEN 'USD' THEN	(T1."U_EXP_IMPORTE" * T5."Rate")
					WHEN 'SOL' THEN (T1."U_EXP_IMPORTE")
				  END)
				,2) 
		AS DECIMAL(10,2)) as "Monto"
	FROM 
	"@EXP_OPMP" T0
	INNER JOIN "@EXP_PMP1" 	T1 ON T1."DocEntry"=T0."DocEntry"
	INNER JOIN "OPCH" 		T2 ON T2."DocEntry"=T1."U_EXP_DOCENTRYDOC" AND T1."U_EXP_TIPODOC" = T2."ObjType"
	INNER JOIN "OCRD" 		T3 ON T3."CardCode"=T2."CardCode"
	--LEFT JOIN "@SMC_APM_3RETCONF" T4 ON T4."U_SMC_TIPODOC" = T2."Indicator"
	INNER JOIN "ORTT" T5 ON T5."RateDate"=T0."U_EXP_FECHA" AND T5."Currency"='USD' 
	WHERE 
	IFNULL(T2."U_EXX_NUMEREND",'')='' -- No considera documentos de caja chica ni rendiciones
	AND T2."Indicator" IN ('02') -- Recibo por HonorarioAND T0."DocEntry"=:docEntry
	--AND ifnull(T1."U_EXP_SLC_PAGO",'') = 'Y'
	AND T0."DocEntry"=:docEntry
	AND T3."U_EXX_TIPODOCU"='6') RH
	GROUP BY RH."RUC"
	HAVING SUM(RH."Monto")>((SELECT MAX("Rate") FROM ORTT WHERE "Currency"='UIT' AND YEAR("RateDate")=YEAR((SELECT "U_EXP_FECHA" FROM "@EXP_OPMP" WHERE "DocEntry"=:docEntry))))
) F
GROUP BY F."RUC";
	
END;