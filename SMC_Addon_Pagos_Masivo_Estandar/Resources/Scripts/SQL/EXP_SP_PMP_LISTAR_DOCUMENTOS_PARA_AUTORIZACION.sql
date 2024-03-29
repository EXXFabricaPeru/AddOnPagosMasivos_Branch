CREATE PROCEDURE EXP_SP_PMP_LISTAR_DOCUMENTOS_PARA_AUTORIZACION
(
	@codAutorizador varchar(50),
	@fechaDesde date,
	@fechaHasta date,
	@cntAutorizaciones int,
	@ventana varchar(1)
)
AS
BEGIN
	select distinct
		'P' 						as "Accion",
		T0."Creator"				as "Creador",
		T0."CreateDate"				as "FechaCreacion",
		'E'							as "TipoDocumento",
		T0."DocNum"					as "NumDoc",
		T0."DocEntry"				as "Codigo",
		isnull(T0."U_CNT_AUT",0)	as "CntActualAut"
	from "@EXD_OEPG" 	T0
	,"@EXD_PM_CONFAUT"	T3
	inner join "@EXD_PM_CONFAUT1"	T4 on T3."Code" = T4."Code"
	where
	T0."U_ESTADO" = 'E'
	and 
	(
	 	T4."U_CODAUTORI" = @codAutorizador or
		case when isnull(T3."U_TIENEAUTBKP",'') = 'Y' then T4."U_CODAUTBKP" else T4."U_CODAUTORI" end  = @codAutorizador
	)
	and T0."CreateDate" between @fechaDesde and @fechaHasta
	and isnull(T0."U_CNT_AUT",0) = @cntAutorizaciones
	and T3."U_VENTANA" = @ventana
	
	union all 
	
	select distinct
		'P' 						as "Accion",
		T0."Creator"				as "Creador",
		T0."CreateDate"				as "FechaCreacion",
		'P'							as "TipoDocumento",
		T0."DocNum"					as "NumDoc",
		T0."DocEntry"				as "Codigo",
		isnull(T0."U_EXP_CNTAUT",0)	as "CntActualAut"
	from "@EXP_OPMP" T0
	,"@EXD_PM_CONFAUT"	T1
	inner join "@EXD_PM_CONFAUT1"	T2 on T1."Code" = T2."Code"
	where 
	T0."U_EXP_ESTADO" = 'E'
	and
	(
	 	T2."U_CODAUTORI" = @codAutorizador or
		case when isnull(T1."U_TIENEAUTBKP",'') = 'Y' then T2."U_CODAUTBKP" else T2."U_CODAUTORI" end  = @codAutorizador
	)
	and T0."CreateDate" between @fechaDesde and @fechaHasta
	and isnull(T0."U_EXP_CNTAUT",0) = @cntAutorizaciones
	and T1."U_VENTANA" = @ventana;
END