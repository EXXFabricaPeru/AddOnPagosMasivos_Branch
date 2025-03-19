CREATE PROCEDURE EXD_PM_VALIDAR_PAGO_RETENCIONES()
AS
BEGIN
	declare tieneSucursales varchar(1);
	select "MltpBrnchs" into tieneSucursales from OADM;

	IF :tieneSucursales = 'Y'
	THEN
		select distinct T0."DocNum" from "@EXP_OPMP" T0 
		inner join "@EXP_PMP1" 	T1 on T0."DocEntry" = T1."DocEntry" 
		inner join OBPL			T2 on T1.U_EXP_COD_SUCURSAL = T2."BPLId"
		where U_EXP_AFECTO_RETENCION = 'Y' and coalesce(T1.U_EXP_ESTADO,'') = ''
		and coalesce(T2.U_EXX_RETPRO,'') =  'Y' and DAYS_BETWEEN(T0."CreateDate",NOW()) > 7;
	ELSE
		select distinct T0."DocNum" from "@EXP_OPMP" T0 
		inner join "@EXP_PMP1" 	T1 on T0."DocEntry" = T1."DocEntry" 		
		where U_EXP_AFECTO_RETENCION = 'Y' and coalesce(T1.U_EXP_ESTADO,'') = ''
		and coalesce((select max(U_VALOR) from "@SMC_APM_CONFIAPM" where "Code" = '2'),'') =  'Y' 
		and DAYS_BETWEEN(T0."CreateDate",NOW()) > 7;
	END IF;
END


