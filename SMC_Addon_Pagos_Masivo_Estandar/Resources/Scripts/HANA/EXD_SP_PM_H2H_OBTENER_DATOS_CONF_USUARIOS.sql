CREATE PROCEDURE EXD_SP_PM_H2H_OBTENER_DATOS_CONF_USUARIOS()
AS
BEGIN
	select 
		T0."BPLId" 			as "CodSucursal",
		T1."U_COD_EMPRESA"	as "CodEmpresa",
		T1."U_USUARIO"		as "Usuario"
	from OBPL T0
	left join "@EXD_PM_CNFUSUH2H" T1 on T0."BPLId" = T1."U_COD_SUCURSAL"
	order by 1;
END;