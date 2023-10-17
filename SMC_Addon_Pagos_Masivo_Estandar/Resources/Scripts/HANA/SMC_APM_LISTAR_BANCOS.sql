CREATE PROCEDURE SMC_APM_LISTAR_BANCOS
(
	codBanco varchar(10)
)
AS
BEGIN
	SELECT 
		T1."Segment_0" || '-' || T1."Segment_1" || '-' ||  T1."Segment_1" AS "CuentaCont",
		T1."AcctName",
		T0."BankCode",
		--T0."Branch" AS "BankName",
		T2."BankName",
		CASE T0."BankCode"
			WHEN '011' THEN
				SUBSTRING(T0."Account",0,10) || '00' || 
				SUBSTRING(T0."Account",11,LENGTH(T0."Account")) 
			ELSE
				T0."Account"
		END AS "CuentaBank",
		T1."ActCurr" AS "Moneda",
		CAST(T1."CurrTotal" AS DECIMAL(16,2)) AS "Saldo"
	FROM 
		DSC1 T0
		LEFT JOIN OACT T1 ON T0."GLAccount" = T1."AcctCode"
		INNER JOIN ODSC T2 ON T0."BankCode"=T2."BankCode"
	WHERE
		T2."BankCode" = :codBanco 
		and IfNULL(T0."U_EXM_PMASIVO",'N') = 'Y'
		ORDER BY T1."AcctName";
END