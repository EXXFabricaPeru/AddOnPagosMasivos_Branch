CREATE PROCEDURE EXD_SP_PM_LISTAR_PAGOS_X_REBOTE
(
	docEntryPM varchar(10),
	codSucurdal int,
	codBanco varchar(10),
	codProveedor varchar(50)
)
AS
BEGIN
	select 
		T1."U_EXP_COD_SUCURSAL",
		T1."U_EXP_CODBANCO",
		T1."U_EXP_CARDCODE",
		T1."U_EXP_CARDNAME",
		T1."U_EXP_DOCENTRYDOC",
		T1."U_EXP_TIPODOC",
		T1."U_EXP_NROSUNAT",
		T1."U_EXP_IMPORTE",
		T1."U_EXP_MONEDA",
		T1."U_EXP_NROPGOEFEC",
		T1."U_EXP_NROPGOEFEC2",
		T1."LineId",
		T1."U_EXP_COD_ESCENARIOPAGO",
		T1."U_EXP_NROLINEA_EP"
	from "@EXP_OPMP" T0 
	inner join "@EXP_PMP1" T1 on T0."DocEntry" = T1."DocEntry"
	where 
	T1."U_EXP_ESTADO" = 'OK'
	and T1."U_EXP_ESTADO2" = 'OK'
	and T0."DocEntry" = :docEntryPM
	and T1."U_EXP_COD_SUCURSAL"  = case when :codSucurdal = '-1' then  T1."U_EXP_COD_SUCURSAL" else :codSucurdal end
	and T1."U_EXP_CODBANCO" = case when ifnull(:codBanco,'') = '' then  T1."U_EXP_CODBANCO" else :codBanco end
	and T1."U_EXP_CARDCODE" = case when ifnull(:codProveedor,'') = '' then  T1."U_EXP_CARDCODE" else :codProveedor end;
END;