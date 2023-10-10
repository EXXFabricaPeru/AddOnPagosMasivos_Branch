CREATE PROCEDURE "SMC_APM_CONSULTAR_BANCO"
( CODIGO VARCHAR(15)
)
AS
BEGIN

	SELECT
		T1."Segment_0" || '-' || T1."Segment_1" || '-'||  T1."Segment_1" AS "CuentaCont",
		T1."AcctName",
		T2."BankCode",
		T2."BankName",
		CASE T0."BankCode"
			WHEN '011' THEN
				SUBSTRING(T0."Account",0,10) || '00' || 
				SUBSTRING(T0."Account",11,LENGTH(T0."Account")) 
			ELSE
				T0."Account"
		END AS "CuentaBank",
		T1."ActCurr" AS "Moneda",
		CAST(T1."CurrTotal" AS DECIMAL(16,2)) AS "Saldo",
		(SELECT "U_SMC_FECHA" FROM "@SMC_APM_ESCCAB" WHERE "Code" = :CODIGO) AS "FechaPago",
		(SELECT IFNULL("U_EXD_ESTE", 'P') FROM "@SMC_APM_ESCCAB" WHERE "Code" = :CODIGO) AS "Estado",
		(SELECT IFNULL("U_EXD_MEDP", '') FROM "@SMC_APM_ESCCAB" WHERE "Code" = :CODIGO) AS "MedioPago"
	FROM
		OACT T1
		LEFT JOIN DSC1 T0 ON T1."AcctCode" = T0."GLAccount"
		LEFT JOIN ODSC T2 ON T0."BankCode" = T2."BankCode"
	WHERE 
		T1."FormatCode" = 
		(SELECT REPLACE("U_SMC_CTACONT",'-','') FROM "@SMC_APM_ESCCAB" WHERE "Code" = :CODIGO);
END;