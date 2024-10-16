CREATE PROCEDURE EXP_SP_PMP_LISTAR_DOCUMENTOS_PARA_AUTORIZACION
(
	IN codAutorizador varchar(50),
	IN fechaDesde date,
	IN fechaHasta date,
	IN cntAutorizaciones int,
	IN ventana varchar(1),
	IN tipoDocumento varchar(5)
)
AS
BEGIN
	select distinct
		'P' 						as "Accion",
		T0."Creator"				as "Creador",
		T0."CreateDate"				as "FechaCreacion",
		'E'							as "Ventana",
		T3."U_TIPO_DOC"				as "TipoDocumento",
		T0."DocEntry"				as "Codigo",
		T0."DocNum"					as "NumDoc",
		ifnull(T0."U_CNT_AUT",0)	as "CntActualAut",
		REPLICATE(' ',250)			as "Comentarios"
	from "@EXD_OEPG" 	T0
	,"@EXD_PM_CONFAUT"	T3
	inner join "@EXD_PM_CONFAUT1"	T4 on T3."Code" = T4."Code" 
	where
	T0."U_TIPO_DOC" = T3."U_TIPO_DOC"
	and T0."U_ESTADO" = 'E'
	and T0."U_TIPO_DOC" = :tipoDocumento
	and ifnull(T0."Canceled",'') != 'Y'
	and 
	(
	 	T4."U_CODAUTORI" = :codAutorizador or
		case when ifnull(T3."U_TIENEAUTBKP",'') = 'Y' then T4."U_CODAUTBKP" else T4."U_CODAUTORI" end  = :codAutorizador
	)
	and T0."CreateDate" between :fechaDesde and :fechaHasta
	and ifnull(T0."U_CNT_AUT",0) = :cntAutorizaciones
	and 'E' = :ventana
	
	union all 
	
	select distinct
		'P' 						as "Accion",
		T0."Creator"				as "Creador",
		T0."CreateDate"				as "FechaCreacion",
		'P'							as "Ventana",
		T1."U_TIPO_DOC"				as "TipoDocumento",
		T0."DocEntry"				as "Codigo",
		T0."DocNum"					as "NumDoc",
		ifnull(T0."U_EXP_CNTAUT",0)	as "CntActualAut",
		REPLICATE(' ',250)			as "Comentarios"
	from "@EXP_OPMP" T0
	,"@EXD_PM_CONFAUT"	T1
	inner join "@EXD_PM_CONFAUT1"	T2 on T1."Code" = T2."Code"
	where
	T1.U_TIPO_DOC = 'VR'
	and T0."U_EXP_ESTADO" = 'E'
	and ifnull(T0."Canceled",'') != 'Y'
	and T1.U_TIPO_DOC = :tipoDocumento
	and
	(
	 	T2."U_CODAUTORI" = :codAutorizador or
		case when ifnull(T1."U_TIENEAUTBKP",'') = 'Y' then T2."U_CODAUTBKP" else T2."U_CODAUTORI" end  = :codAutorizador
	)
	and T0."CreateDate" between :fechaDesde and :fechaHasta
	and ifnull(T0."U_EXP_CNTAUT",0) = :cntAutorizaciones
	and 'P' = :ventana;
END