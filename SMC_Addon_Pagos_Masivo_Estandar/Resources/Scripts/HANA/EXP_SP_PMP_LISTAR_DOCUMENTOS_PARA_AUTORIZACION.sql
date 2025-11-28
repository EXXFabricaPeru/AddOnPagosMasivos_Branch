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
	
	TMP_AUTORIZADORES = select 
		"Code",map(element_number,1,U_CODAUTORI,2,U_CODAUTBKP,3,U_CODAUTBKP2,4,U_CODAUTBKP3,5,U_CODAUTBKP4,6,U_CODAUTBKP5,7,U_CODAUTBKP6) as "Autorizadores",'Y' as "AutBackup"
	from "@EXD_PM_CONFAUT1",SERIES_GENERATE_INTEGER(1, 1, 8) 
	union all
	select "Code",U_CODAUTORI,'N' from "@EXD_PM_CONFAUT1";	

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
	T0."U_AUTORIZAR_POR" = T3."U_TIPO_DOC"
	and T0."U_ESTADO" = 'E'
	and T0."U_AUTORIZAR_POR" = :tipoDocumento
	and ifnull(T0."Canceled",'') != 'Y'
	and T0."CreateDate" between :fechaDesde and :fechaHasta
	and ifnull(T0."U_CNT_AUT",0) = :cntAutorizaciones
	and 'E' = :ventana
	and :codAutorizador in (select TX0."Autorizadores" from :TMP_AUTORIZADORES TX0 where TX0."Code" = T3."Code" 
	and TX0."AutBackup" = ifnull(T3."U_TIENEAUTBKP",'N'))
	
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
	and T0."CreateDate" between :fechaDesde and :fechaHasta
	and ifnull(T0."U_EXP_CNTAUT",0) = :cntAutorizaciones
	and 'P' = :ventana
	and :codAutorizador in (select TX0."Autorizadores" from :TMP_AUTORIZADORES TX0 where TX0."Code" = T1."Code" 
	and TX0."AutBackup" = ifnull(T1."U_TIENEAUTBKP",'N'));
END