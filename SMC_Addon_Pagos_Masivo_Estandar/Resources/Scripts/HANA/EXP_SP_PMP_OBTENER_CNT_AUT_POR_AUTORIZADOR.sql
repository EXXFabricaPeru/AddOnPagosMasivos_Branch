CREATE PROCEDURE EXP_SP_PMP_OBTENER_CNT_AUT_POR_AUTORIZADOR
(
	IN ventana varchar(1),
	IN codAutorizador varchar(50)
)
AS
BEGIN 
	select ifnull(T1."U_CNTAUTNEC",'0') as "CntAutNec"
	from "@EXD_PM_CONFAUT" 			T0 
	inner join "@EXD_PM_CONFAUT1" 	T1 on T0."Code" = T1."Code"
	where T0."U_VENTANA" = :ventana and (T1."U_CODAUTORI" = :codAutorizador 
	or (case when ifnull(T0."U_TIENEAUTBKP",'') = 'Y' then T1."U_CODAUTBKP" else T1."U_CODAUTORI" end = :codAutorizador)); 
END;