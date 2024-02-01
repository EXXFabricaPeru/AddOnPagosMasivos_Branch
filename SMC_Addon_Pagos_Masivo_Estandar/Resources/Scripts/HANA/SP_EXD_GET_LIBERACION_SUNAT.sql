CREATE PROCEDURE SP_EXD_GET_LIBERACION_SUNAT
(
	IN DocEntryPM INT,
	IN FechaPago DATE
	--,IN TipoCambio  numeric(19,6) 
)
AS
BEGIN
	
	DECLARE MonedaLocal  nvarchar(3);
	DECLARE DecimalesImporte  int;
	DECLARE TipoCambio  decimal(21,6);
	
	SELECT "MainCurncy", "SumDec" INTO MonedaLocal,DecimalesImporte  FROM OADM;
	SELECT "Rate" INTO TipoCambio FROM ORTT WHERE "RateDate" = :FechaPago AND "Currency" = 'USD';
	
	WITH ANTERIORES AS 
	(
		SELECT 	
				U_EXD_NUMI 
				,U_EXD_TDOC
		
		FROM 		"@EXD_OTRR" AS T0
		INNER JOIN  "@EXD_TRR1" AS T1 ON T0."DocEntry" = T1."DocEntry"
		
		WHERE 	T0."U_EXD_FPAG" = :FechaPago 
	)
	
	--SELECT  "U_EXP_MEDIODEPAGO", * FROM "@EXP_PMP1" WHERE "DocEntry" = 5
	
	SELECT 	ROW_NUMBER() OVER(ORDER BY T1."U_EXP_CARDCODE") 	AS "LineId" 
		  	,'Y'												AS "U_EXD_IPAG"
		  	,T1."U_EXP_COD_ESCENARIOPAGO"						AS "U_EXD_CODE"
		  	,T2."BankName"										AS "U_EXD_BAES"
		  	,T2."BankCode"										AS "U_EXD_BKCD"
		  	,T1."U_EXP_CODCTABANCO"								AS "U_EXD_CTAB"
		  	,T1."U_EXP_MEDIODEPAGO"								AS "U_EXD_MEDP"
		  	,T1."U_EXP_CARDCODE"								AS "U_EXD_PROV"
		  	,T1."U_EXP_CARDNAME"								AS "U_EXD_NPRO"
		  	,T1."U_EXP_DOCENTRYDOC"								AS "U_EXD_NUMI"
			,T1."U_EXP_TIPODOC"									AS "U_EXD_TDOC"
			,T1."U_EXP_NROSUNAT"								AS "U_EXD_NSNT"
			,IFNULL(NULLIF(T1.U_EXP_NMROCUOTA,''), '1')			AS "U_EXD_NCUO"
			,''													AS "U_EXD_NUMD"
			,T1."U_EXP_MONEDA"									AS "U_EXD_MONE"
			,T1."U_EXP_IMPORTE"									AS "U_EXD_TOTD"
		  	,CASE T1."U_EXP_MONEDA" 
				WHEN :MonedaLocal THEN  T1."U_EXP_IMPORTE"							
				ELSE ROUND(T1."U_EXP_IMPORTE" * T1."U_EXP_TCDOCUMENTO", :DecimalesImporte)   
			END 												AS "U_EXD_TOTP"
			,CASE T1."U_EXP_MONEDA" 
				WHEN :MonedaLocal THEN  T1."U_EXP_IMPORTE"							
				ELSE ROUND(T1."U_EXP_IMPORTE" * T1."U_EXP_TCDOCUMENTO", :DecimalesImporte)   
			END 												AS "U_EXD_PAGP"
			
			,T1."U_EXP_IMPORTE"									AS "U_EXD_PPME"
			
			,T1."U_EXP_ASNROLINEA"								AS "U_EXD_LIAS"
			,T1."U_EXP_APLSRERTN"								AS "U_EXD_INDR" 
			,T1."U_EXP_CODRETENCION"							AS "U_EXD_CDRE"
			,T1."U_EXP_IMPRETENCION"							AS "U_EXD_RETS"
			,'N'												AS "U_EXD_GEPP"
			,T1."U_EXP_GLOSAASIENTO" 							AS "U_EXD_GSAS"
			,T1."U_EXP_CARDCODE_FACTO"							as "U_EXD_CCFC"
			,T1."U_EXP_CARDNAME_FACTO"							as "U_EXD_CNFC"
			,T1."U_EXP_MONEDA_PAGO"								as "U_EXD_MDPG"
	FROM 		"@EXP_OPMP" AS T0
	INNER JOIN  "@EXP_PMP1" AS T1	ON T0."DocEntry" = T1."DocEntry"
	LEFT  JOIN	"ODSC"		AS T2	ON T1."U_EXP_CODBANCO" = T2."BankCode"
	LEFT  JOIN 	ANTERIORES	AS T3	ON T1.U_EXP_DOCENTRYDOC = T3.U_EXD_NUMI AND T1.U_EXP_TIPODOC = T3.U_EXD_TDOC
	WHERE T0."DocEntry" = :DocEntryPM AND IFNULL(T1."U_EXP_SLC_RETENCION", 'N') = 'Y' AND T3."U_EXD_NUMI" IS NULL
	--ORDER BY T1."U_EXP_CARDCODE"
	; 


END;