CREATE procedure EXP_SP_PMP_ListarDocumentosPorFecha
(
	fechaEscPago date
)
as
begin
	--Factura de proveedor
	RSLT1 = select distinct		
		T0."Code"					as "CodEscenarioPago",
		T0."U_SMC_ESC_DESC"			as "DscEscenarioPago",
		T0."U_EXD_MEDP"				as "MedioDePago",
		T3."BankCode" 				as "CodBanco",
		T2."AcctCode" 				as "CodCtaBanco",
		T0."U_SMC_CTACONT"			as "NumCtaBanco",
		T1."U_SMC_DOCENTRY"			as "DocEntryDocumento",
		T4."ObjType"				as "TipoDocumento",
		T4."DocStatus"				as "EstadoDocumento",
		T4."CardCode"				as "CardCode",
		T4."CardName"				as "CardName",
		T6."LicTradNum"				as "NroDocumentoSN",
		T3."UsrNumber1" 			as "Moneda",
		T1."U_SMC_MONTO"			as "Importe",
		'0'							as "NroLineaAsiento",
		T5."InstlmntID" 			as "NroCuota"
	from "@SMC_APM_ESCCAB" 			T0 
	inner 	join "@SMC_APM_ESCDET" 	T1 on T0."Code" 		= T1."U_SMC_ESCCAB"
	inner 	join OACT 				T2 on T2."FormatCode" 	= replace(T0."U_SMC_CTACONT",'-','')
	inner 	join DSC1				T3 on T3."GLAccount" 	= T2."AcctCode"
	inner 	join OPCH				T4 on T4."DocEntry" 	= trim(T1."U_SMC_DOCENTRY") and T1."U_SMC_TIPO_DOCUMENTO" = 'FT-P'
	inner 	join PCH6 				T5 on T5."DocEntry"		= T4."DocEntry"
	inner 	join OCRD				T6 on T4."CardCode"		= T6."CardCode"
	where  T0."U_SMC_FECHA" = :fechaEscPago and T4."DocStatus" = 'O' and T5."Status" = 'O'

	union all 
	--Nota de credito de cliente
	select distinct		
		T0."Code"					as "CodEscenarioPago",
		T0."U_SMC_ESC_DESC"			as "DscEscenarioPago",
		T0."U_EXD_MEDP"				as "MedioDePago",
		T3."BankCode" 				as "CodBanco",
		T2."AcctCode" 				as "CodCtaBanco",
		T0."U_SMC_CTACONT"			as "NumCtaBanco",
		T1."U_SMC_DOCENTRY"			as "DocEntryDocumento",
		T4."ObjType"				as "TipoDocumento",
		T4."DocStatus"				as "EstadoDocumento",
		T4."CardCode"				as "CardCode",
		T4."CardName"				as "CardName",
		T5."LicTradNum"				as "NroDocumentoSN",
		T3."UsrNumber1" 			as "Moneda",
		T1."U_SMC_MONTO"			as "Importe",
		'0'							as "NroLineaAsiento",
		'0'							as "NroCuota"
	from "@SMC_APM_ESCCAB" 			T0 
	inner 	join "@SMC_APM_ESCDET" 	T1 on T0."Code" 		= T1."U_SMC_ESCCAB"
	inner 	join OACT 				T2 on T2."FormatCode" 	= replace(T0."U_SMC_CTACONT",'-','')
	inner 	join DSC1				T3 on T3."GLAccount" 	= T2."AcctCode"
	inner  	join ORIN				T4 on T4."DocEntry" 	= trim(T1."U_SMC_DOCENTRY") and T1."U_SMC_TIPO_DOCUMENTO" = 'NC-C'
	inner 	join OCRD				T5 on T4."CardCode"		= T5."CardCode" 
	where  T0."U_SMC_FECHA" = :fechaEscPago and T4."DocStatus" = 'O'
	
	union all
	--Anticipo de proveedor
	select distinct		
		T0."Code"					as "CodEscenarioPago",
		T0."U_SMC_ESC_DESC"			as "DscEscenarioPago",
		T0."U_EXD_MEDP"				as "MedioDePago",
		T3."BankCode" 				as "CodBanco",
		T2."AcctCode" 				as "CodCtaBanco",
		T0."U_SMC_CTACONT"			as "NumCtaBanco",
		T1."U_SMC_DOCENTRY"			as "DocEntryDocumento",
		T4."ObjType"				as "TipoDocumento",
		T4."DocStatus"				as "EstadoDocumento",
		T4."CardCode"				as "CardCode",
		T4."CardName"				as "CardName",
		T5."LicTradNum"				as "NroDocumentoSN",
		T3."UsrNumber1" 			as "Moneda",
		T1."U_SMC_MONTO"			as "Importe",
		'0'							as "NroLineaAsiento",
		'0'							as "NroCuota"
	from "@SMC_APM_ESCCAB" 			T0 
	inner 	join "@SMC_APM_ESCDET" 	T1 on T0."Code" 		= T1."U_SMC_ESCCAB"
	inner 	join OACT 				T2 on T2."FormatCode" 	= replace(T0."U_SMC_CTACONT",'-','')
	inner 	join DSC1				T3 on T3."GLAccount" 	= T2."AcctCode"
	inner 	join ODPO				T4 on T4."DocEntry" 	= trim(T1."U_SMC_DOCENTRY") and T1."U_SMC_TIPO_DOCUMENTO" IN ('FA-P','SA-P')
	inner 	join OCRD				T5 on T4."CardCode"		= T5."CardCode" 
	where  T0."U_SMC_FECHA" = :fechaEscPago and T4."DocStatus" = 'O'
	
	union all
	--Pago borrador
	select distinct		
		T0."Code"					as "CodEscenarioPago",
		T0."U_SMC_ESC_DESC"			as "DscEscenarioPago",
		T0."U_EXD_MEDP"				as "MedioDePago",
		T3."BankCode" 				as "CodBanco",
		T2."AcctCode" 				as "CodCtaBanco",
		T0."U_SMC_CTACONT"			as "NumCtaBanco",
		T1."U_SMC_DOCENTRY"			as "DocEntryDocumento",
		'140'						as "TipoDocumento",
		'O'							as "EstadoDocumento",
		T4."CardCode"				as "CardCode",
		T4."CardName"				as "CardName",
		T5."LicTradNum"				as "NroDocumentoSN",
		T3."UsrNumber1" 			as "Moneda",
		T1."U_SMC_MONTO"			as "Importe",
		'0'							as "NroLineaAsiento",
		'0'							as "NroCuota"
	from "@SMC_APM_ESCCAB" 			T0 
	inner 	join "@SMC_APM_ESCDET" 	T1 on T0."Code" 		= T1."U_SMC_ESCCAB"
	inner 	join OACT 				T2 on T2."FormatCode" 	= replace(T0."U_SMC_CTACONT",'-','')
	inner 	join DSC1				T3 on T3."GLAccount" 	= T2."AcctCode"
	inner 	join OPDF				T4 on T4."DocEntry" 	= trim(T1."U_SMC_DOCENTRY") and T1."U_SMC_TIPO_DOCUMENTO" = 'SP'
	inner 	join OCRD				T5 on T4."CardCode"		= T5."CardCode"
	where  T0."U_SMC_FECHA" = :fechaEscPago and T4."ObjType" = '46' and T4."Canceled" = 'N'
	
	union all
	--Asiento
	select distinct		
		T0."Code"					as "CodEscenarioPago",
		T0."U_SMC_ESC_DESC"			as "DscEscenarioPago",
		T0."U_EXD_MEDP"				as "MedioDePago",
		T3."BankCode" 				as "CodBanco",
		T2."AcctCode" 				as "CodCtaBanco",
		T0."U_SMC_CTACONT"			as "NumCtaBanco",
		T1."U_SMC_DOCENTRY"			as "DocEntryDocumento",
		T4."ObjType"				as "TipoDocumento",
		'O'							as "EstadoDocumento",
		T4."ShortName"				as "CardCode",
		T5."CardName"				as "CardName",
		T5."LicTradNum"				as "NroDocumentoSN",
		T3."UsrNumber1" 			as "Moneda",
		T1."U_SMC_MONTO"			as "Importe",
		T4."Line_ID"				as "NroLineaAsiento",
		'0'							as "NroCuota"
	from "@SMC_APM_ESCCAB" 			T0 
	inner 	join "@SMC_APM_ESCDET" 	T1 on T0."Code" 		= T1."U_SMC_ESCCAB"
	inner 	join OACT 				T2 on T2."FormatCode" 	= replace(T0."U_SMC_CTACONT",'-','')
	inner 	join DSC1				T3 on T3."GLAccount" 	= T2."AcctCode"
	inner 	join JDT1				T4 on T4."TransId" 		= trim(T1."U_SMC_DOCENTRY") and  T1."U_SMC_TIPO_DOCUMENTO" = 'AS'
	inner 	join OCRD				T5 on T5."CardCode"		= T4."ShortName"							 
	where  T0."U_SMC_FECHA" = 		:fechaEscPago and T1."U_EXP_LINEAASIENTO" = T4."Line_ID" and T5."CardType" = 'S' and T4."DebCred" = 'C'
	
	union all  
	--Pago recibido
	select DISTINCT 		
		T0."Code"					as "CodEscenarioPago",
		T0."U_SMC_ESC_DESC"			as "DscEscenarioPago",
		T0."U_EXD_MEDP"				as "MedioDePago",
		T3."BankCode" 				as "CodBanco",
		T2."AcctCode" 				as "CodCtaBanco",
		T0."U_SMC_CTACONT"			as "NumCtaBanco",
		T1."U_SMC_DOCENTRY"			as "DocEntryDocumento",
		'24'						as "TipoDocumento",
		'O'							as "EstadoDocumento",
		T4."ShortName"				as "CardCode",
		T5."CardName"				as "CardName",
		T5."LicTradNum"				as "NroDocumentoSN",
		T3."UsrNumber1" 			as "Moneda",
		T1."U_SMC_MONTO"			as "Importe",
		T4."Line_ID"				as "NroLineaAsiento",
		'0'							as "NroCuota"
	from "@SMC_APM_ESCCAB" 			T0 
	inner 	join "@SMC_APM_ESCDET" 	T1 on T0."Code" 		= T1."U_SMC_ESCCAB"
	inner 	join OACT 				T2 on T2."FormatCode" 	= replace(T0."U_SMC_CTACONT",'-','')
	inner 	join DSC1				T3 on T3."GLAccount" 	= T2."AcctCode"
	inner 	join JDT1				T4 on T4."TransId" 		= trim(T1."U_SMC_DOCENTRY") and  T1."U_SMC_TIPO_DOCUMENTO" = 'PR'
	inner 	join OCRD				T5 on T5."CardCode"		= T4."ShortName"								 
	where  T0."U_SMC_FECHA" = 		:fechaEscPago and T1."U_EXP_LINEAASIENTO" = T4."Line_ID" and T5."CardType" = 'C' and T4."DebCred" = 'C' ;
	

	RSLT2 = select 
		coalesce(T1."ObjType",T2."ObjType",T3."ObjType")	as "ObjType",
		coalesce(T1."AbsEntry",T2."AbsEntry",T3."AbsEntry") as "DocEntry"
	from 
	OWHT T0 
	left join PCH5 T1 on T0."WTCode" = T1."WTCode"
	left join RIN5 T2 on T0."WTCode" = T2."WTCode"
	left join DPO5 T3 on T0."WTCode" = T3."WTCode"
	where ifnull(coalesce(T1."ObjType",T2."ObjType",T3."ObjType"),'0')  <> '0'
	and ifnull(coalesce(T1."AbsEntry",T2."AbsEntry",T3."AbsEntry"),'0') <> '0';

	select 
		'Y'							as "SlcPago",
		'N'							as "SlcRetencion",
		T0."CodEscenarioPago",
		T0."DscEscenarioPago",
		T0."MedioDePago",
		T0."CodBanco",
		T0."CodCtaBanco",
		T0."NumCtaBanco",
		T0."DocEntryDocumento",		
		T0."TipoDocumento",
		T0."EstadoDocumento",
		T0."CardCode",
		T0."CardName",
		T0."Moneda",
		T0."Importe",
		T0."NroCuota",
		T0."NroLineaAsiento",
		T0."NroDocumentoSN",
		ifnull((select max('Y') from :RSLT2 TX0 where TX0."ObjType" = T0."TipoDocumento" 
		and TX0."DocEntry"= T0."DocEntryDocumento" ),'N') as "AplSerieRetencion"
	from :RSLT1 T0;
end