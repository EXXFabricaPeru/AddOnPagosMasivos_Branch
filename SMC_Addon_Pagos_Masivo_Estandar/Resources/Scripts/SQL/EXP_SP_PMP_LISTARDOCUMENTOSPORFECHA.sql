CREATE PROCEDURE EXP_SP_PMP_LISTARDOCUMENTOSPORFECHA --'20231013'
(
	@fechaEscPago date
)
as
begin
	--Factura de proveedor
	WITH RSLT1
	as
	(
		 select distinct		
			T0."DocEntry"				as "CodEscenarioPago",
			T0."DocNum"					as "NumEscenarioPago",
			''							as "DscEscenarioPago",
			T0."U_MEDIO_DE_PAGO"		as "MedioDePago",
			T1."U_MONEDA_PAGO"			as "MonedaDePago",
			T3."BankCode" 				as "CodBanco",
			T2."AcctCode" 				as "CodCtaBanco",
			case when T1."U_MONEDA_PAGO" = 'SOL' then  T0."U_CTA_BANCO_ML" else T0."U_CTA_BANCO_ME" end as "NumCtaBanco",
			T1."U_DOCENTRY"				as "DocEntryDocumento",
			T4."ObjType"				as "TipoDocumento",
			T4."FolioPref"+'-'+cast(T4."FolioNum" as varchar) as "NroDocSUNAT",
			T4."DocStatus"				as "EstadoDocumento",
			T4."CardCode"				as "CardCode",
			T4."CardName"				as "CardName",
			T6."LicTradNum"				as "NroDocumentoSN",
			T4."DocCur" 				as "Moneda",
			T1."U_TOTAL_PAGO"			as "Importe",
			'0'							as "NroLineaAsiento",
			T5."InstlmntID" 			as "NroCuota",
			T1."U_CUENTA_PROV"			as "NroCtaProveedor",
			T1."U_COD_BANCO"			as "CodBncProveedor",
			T7."WTCode"					as "CodRetencion",
			case when T4."DocCur" = 'SOL' then 1 * (T7."WTAmnt" - T7."ApplAmnt")
			else 1 * (T7."WTAmntFC" - T7."ApplAmntFC") end as "MontoRetencion",
			T4."DocRate"				as "TCDocumento",
			T4."JrnlMemo"				as "GlosaAsiento",
			T1."U_COD_PROV_FACTO"		as "CardCodeFacto",
			T1."U_NOM_PROV_FACTO"		as "CardNameFacto"
		from "@EXD_OEPG" 				T0 
		inner 	join "@EXD_EPG1" 		T1 on T0."DocEntry" 		= T1."DocEntry"
		inner 	join OACT 				T2 on T2."FormatCode" 	= replace(case when T1.U_MONEDA_PAGO = 'SOL' then  T0."U_CTA_BANCO_ML" else T0."U_CTA_BANCO_ME" end,'-','')
		inner 	join DSC1				T3 on T3."GLAccount" 	= T2."AcctCode"
		inner 	join OPCH				T4 on T4."DocEntry" 	= rtrim(ltrim(T1."U_DOCENTRY")) and T1."U_TIPO_DOCUMENTO" = 'FT-P'
		inner 	join PCH6 				T5 on T5."DocEntry"		= T4."DocEntry" AND T5."InstlmntID"=T1."U_NRO_CUOTA"
		inner 	join OCRD				T6 on T4."CardCode"		= T6."CardCode"
		left 	join PCH5				T7 on T4."DocEntry"		= T7. "AbsEntry"
		where  T0."U_FECHA_PAGO" = @fechaEscPago and ISNULL(T4."DocStatus",'') != 'C' and ISNULL(T4."CANCELED",'') = 'N' 
		and T5."Status" = 'O' and T0."U_ESTADO"='A'

		union all 
		--Nota de credito de cliente
		select distinct		
			T0."DocEntry"				as "CodEscenarioPago",
			T0."DocNum"					as "NumEscenarioPago",
			''							as "DscEscenarioPago",
			T0."U_MEDIO_DE_PAGO"		as "MedioDePago",
			T1."U_MONEDA_PAGO"			as "MonedaDePago",
			T3."BankCode" 				as "CodBanco",
			T2."AcctCode" 				as "CodCtaBanco",
			case when T1."U_MONEDA_PAGO" = 'SOL' then  T0."U_CTA_BANCO_ML" else T0."U_CTA_BANCO_ME" end as "NumCtaBanco",
			T1."U_DOCENTRY"				as "DocEntryDocumento",
			T4."ObjType"				as "TipoDocumento",
			T4."FolioPref" +'-'+cast(T4."FolioNum" as varchar) as "NroDocSUNAT",
			T4."DocStatus"				as "EstadoDocumento",
			T4."CardCode"				as "CardCode",
			T4."CardName"				as "CardName",
			T5."LicTradNum"				as "NroDocumentoSN",
			T3."UsrNumber1" 			as "Moneda",
			T1."U_TOTAL_PAGO"			as "Importe",
			'0'							as "NroLineaAsiento",
			'0'							as "NroCuota",
			T1."U_CUENTA_PROV"			as "NroCtaProveedor",
			T1."U_COD_BANCO"			as "CodBncProveedor",
			T6."WTCode"					as "CodRetencion",
			case when T4."DocCur" = 'SOL' then 1 * (T6."WTAmnt" - T6."ApplAmnt") 
			else 1 * (T6."WTAmntFC" - T6."ApplAmntFC") end as "MontoRetencion",
			T4."DocRate"				as "TCDocumento",
			T4."JrnlMemo"				as "GlosaAsiento",
			T1."U_COD_PROV_FACTO"		as "CardCodeFacto",
			T1."U_NOM_PROV_FACTO"		as "CardNameFacto"
		from "@EXD_OEPG" 				T0 
		inner 	join "@EXD_EPG1" 		T1 on T0."DocEntry" 	= T1."DocEntry"
		inner 	join OACT 				T2 on T2."FormatCode" 	= replace(case when T1.U_MONEDA_PAGO = 'SOL' then  T0."U_CTA_BANCO_ML" else T0."U_CTA_BANCO_ME" end,'-','')
		inner 	join DSC1				T3 on T3."GLAccount" 	= T2."AcctCode"
		inner  	join ORIN				T4 on T4."DocEntry" 	= rtrim(ltrim(T1."U_DOCENTRY")) and T1."U_TIPO_DOCUMENTO" = 'NC-C'
		inner 	join OCRD				T5 on T4."CardCode"		= T5."CardCode"
		left 	join RIN5				t6 on T6."AbsEntry"		= T4."DocEntry" 
		where  T0."U_FECHA_PAGO" = @fechaEscPago and T4."DocStatus" = 'O' AND T0."U_ESTADO" = 'A'
	
		union all
		--Anticipo de proveedor
		select distinct		
			T0."DocEntry"				as "CodEscenarioPago",
			T0."DocNum"					as "NumEscenarioPago",
			''							as "DscEscenarioPago",
			T0."U_MEDIO_DE_PAGO"		as "MedioDePago",
			T1."U_MONEDA_PAGO"			as "MonedaDePago",
			T3."BankCode" 				as "CodBanco",
			T2."AcctCode" 				as "CodCtaBanco",
			case when T1."U_MONEDA_PAGO" = 'SOL' then  T0."U_CTA_BANCO_ML" else T0."U_CTA_BANCO_ME" end as "NumCtaBanco",
			T1."U_DOCENTRY"				as "DocEntryDocumento",
			T4."ObjType"				as "TipoDocumento",
			T4."FolioPref"+'-'+cast(T4."FolioNum" as varchar) as "NroDocSUNAT",
			T4."DocStatus"				as "EstadoDocumento",
			T4."CardCode"				as "CardCode",
			T4."CardName"				as "CardName",
			T5."LicTradNum"				as "NroDocumentoSN",
			T3."UsrNumber1" 			as "Moneda",
			T1."U_TOTAL_PAGO"			as "Importe",
			'0'							as "NroLineaAsiento",
			'1'							as "NroCuota",
			T1."U_CUENTA_PROV"			as "NroCtaProveedor",
			T1."U_COD_BANCO"			as "CodBncProveedor",
			''							as "CodRetencion",
			0 							as "MontoRetencion",
			T4."DocRate"				as "TCDocumento",
			T4."JrnlMemo"				as "GlosaAsiento",
			T1."U_COD_PROV_FACTO"		as "CardCodeFacto",
			T1."U_NOM_PROV_FACTO"		as "CardNameFacto"	
		from "@EXD_OEPG" 				T0 
		inner 	join "@EXD_EPG1" 		T1 on T0."DocEntry" 	= T1."DocEntry"
		inner 	join OACT 				T2 on T2."FormatCode" 	= replace(case when T1.U_MONEDA_PAGO = 'SOL' then  T0."U_CTA_BANCO_ML" else T0."U_CTA_BANCO_ME" end,'-','')
		inner 	join DSC1				T3 on T3."GLAccount" 	= T2."AcctCode"
		inner 	join ODPO				T4 on T4."DocEntry" 	= rtrim(ltrim(T1."U_DOCENTRY")) and T1."U_TIPO_DOCUMENTO" IN ('FA-P','SA-P')
		inner 	join OCRD				T5 on T4."CardCode"		= T5."CardCode"
		--inner 	join DPO5				T6 on T6."AbsEntry"		= T4."DocEntry"
		where  T0."U_FECHA_PAGO" = @fechaEscPago and T4."DocStatus" = 'O' AND T0."U_ESTADO"='A'
	
		union all
		--Pago borrador
		select distinct		
			T0."DocEntry"				as "CodEscenarioPago",
			T0."DocNum"					as "NumEscenarioPago",
			''							as "DscEscenarioPago",
			T0."U_MEDIO_DE_PAGO"		as "MedioDePago",
			T1."U_MONEDA_PAGO"			as "MonedaDePago",
			T3."BankCode" 				as "CodBanco",
			T2."AcctCode" 				as "CodCtaBanco",
			case when T1."U_MONEDA_PAGO" = 'SOL' then  T0."U_CTA_BANCO_ML" else T0."U_CTA_BANCO_ME" end as "NumCtaBanco",
			T1."U_DOCENTRY"				as "DocEntryDocumento",
			'140'						as "TipoDocumento",
			'' 							as "NroDocSUNAT",
			'O'							as "EstadoDocumento",
			T4."CardCode"				as "CardCode",
			T4."CardName"				as "CardName",
			T5."LicTradNum"				as "NroDocumentoSN",
			T3."UsrNumber1" 			as "Moneda",
			T1."U_TOTAL_PAGO"			as "Importe",
			'0'							as "NroLineaAsiento",
			'0'							as "NroCuota",
			T1."U_CUENTA_PROV"			as "NroCtaProveedor",
			T1."U_COD_BANCO"			as "CodBncProveedor",
			''							as "CodRetencion",
			0							as "MontoRetencion",
			0							as "TCDocumento",
			T4."JrnlMemo"				as "GlosaAsiento",
			T1."U_COD_PROV_FACTO"		as "CardCodeFacto",
			T1."U_NOM_PROV_FACTO"		as "CardNameFacto"
		from "@EXD_OEPG" 				T0 
		inner 	join "@EXD_EPG1" 		T1 on T0."DocEntry" 	= T1."DocEntry"
		inner 	join OACT 				T2 on T2."FormatCode" 	= replace(case when T1.U_MONEDA_PAGO = 'SOL' then  T0."U_CTA_BANCO_ML" else T0."U_CTA_BANCO_ME" end,'-','')
		inner 	join DSC1				T3 on T3."GLAccount" 	= T2."AcctCode"
		inner 	join OPDF				T4 on T4."DocEntry" 	= rtrim(ltrim(T1."U_DOCENTRY")) and T1."U_TIPO_DOCUMENTO" = 'SP'
		inner 	join OCRD				T5 on T4."CardCode"		= T5."CardCode"
		where  T0."U_FECHA_PAGO" = @fechaEscPago and T4."ObjType" = '46' and T4."Canceled" = 'N' AND T0."U_ESTADO"='A'
	
		union all
		--Asiento
		select distinct		
			T0."DocEntry"				as "CodEscenarioPago",
			T0."DocNum"					as "NumEscenarioPago",
			''							as "DscEscenarioPago",
			T0."U_MEDIO_DE_PAGO"		as "MedioDePago",
			T1."U_MONEDA_PAGO"			as "MonedaDePago",
			T3."BankCode" 				as "CodBanco",
			T2."AcctCode" 				as "CodCtaBanco",
			case when T1."U_MONEDA_PAGO" = 'SOL' then  T0."U_CTA_BANCO_ML" else T0."U_CTA_BANCO_ME" end	as "NumCtaBanco",
			T1."U_DOCENTRY"			as "DocEntryDocumento",
			T4."ObjType"				as "TipoDocumento",
			'' 							as "NroDocSUNAT",
			'O'							as "EstadoDocumento",
			T4."ShortName"				as "CardCode",
			T5."CardName"				as "CardName",
			T5."LicTradNum"				as "NroDocumentoSN",
			T3."UsrNumber1" 			as "Moneda",
			T1."U_TOTAL_PAGO"			as "Importe",
			T4."Line_ID"				as "NroLineaAsiento",
			'0'							as "NroCuota",
			T1."U_CUENTA_PROV"			as "NroCtaProveedor",
			T1."U_COD_BANCO"			as "CodBncProveedor",
			T6."WTCode"					as "CodRetencion",
			case when ISNULL(T4."FCCurrency",'') = '' then 1 * (T6."WTAmnt" - T6."ApplAmnt") 
			else 1 * (T6."WTAmntFC" - T6."ApplAmntFC") end as "MontoRetencion",
			T4."FCCredit"/(case when T4."Credit" = 0 then 1 else T4."Credit" end) as "TCDocumento",
			(select TX0."Memo" from OJDT TX0 where TX0."TransId" = T4."TransId")	as "GlosaAsiento",
			T1."U_COD_PROV_FACTO"		as "CardCodeFacto",
			T1."U_NOM_PROV_FACTO"		as "CardNameFacto"
		from "@EXD_OEPG" 				T0 
		inner 	join "@EXD_EPG1" 		T1 on T0."DocEntry" 	= T1."DocEntry"
		inner 	join OACT 				T2 on T2."FormatCode" 	= replace(case when T1.U_MONEDA_PAGO = 'SOL' then  T0."U_CTA_BANCO_ML" else T0."U_CTA_BANCO_ME" end,'-','')
		inner 	join DSC1				T3 on T3."GLAccount" 	= T2."AcctCode"
		inner 	join JDT1				T4 on T4."TransId" 		= rtrim(ltrim(T1."U_DOCENTRY")) and T1."U_TIPO_DOCUMENTO" = 'AS'
		inner 	join OCRD				T5 on T5."CardCode"		= T4."ShortName"	
		left 	join JDT2				T6 on T4."TransId"		= T6."AbsEntry"						 
		where  T0."U_FECHA_PAGO" = 		@fechaEscPago 
		and T1."U_NRO_LINEA_AS" = T4."Line_ID" 
		and T5."CardType" = 'S' and T4."DebCred" = 'C' AND T0."U_ESTADO" = 'A'
		and (select ISNULL(max(TX0."U_EXP_ESTADO"),'') from "@EXP_PMP1" TX0 
		--inner join OVPM TX1 on TX0."U_EXP_NROPGOEFEC" = TO_VARCHAR(TX1."DocEntry") 
		where TX0."U_EXP_TIPODOC" = T4."ObjType" and TX0."U_EXP_DOCENTRYDOC" = T4."TransId" 
		and TX0."U_EXP_ASNROLINEA" = T4."Line_ID" /*and ISNULL(TX1."Canceled",'') = 'N'*/
		and TX0."U_EXP_COD_ESCENARIOPAGO" = T0."DocEntry") != 'OK'
	
		union all  
		--Pago recibido
		select DISTINCT 		
			T0."DocEntry"				as "CodEscenarioPago",
			T0."DocNum"					as "NumEscenarioPago",
			''							as "DscEscenarioPago",
			T0."U_MEDIO_DE_PAGO"		as "MedioDePago",
			T1."U_MONEDA_PAGO"			as "MonedaDePago",
			T3."BankCode" 				as "CodBanco",
			T2."AcctCode" 				as "CodCtaBanco",
			case when T1."U_MONEDA_PAGO" = 'SOL' then  T0."U_CTA_BANCO_ML" else T0."U_CTA_BANCO_ME" end	as "NumCtaBanco",
			T1."U_DOCENTRY"				as "DocEntryDocumento",
			'24'						as "TipoDocumento",
			'' 							as "NroDocSUNAT",
			'O'							as "EstadoDocumento",
			T4."ShortName"				as "CardCode",
			T5."CardName"				as "CardName",
			T5."LicTradNum"				as "NroDocumentoSN",
			T3."UsrNumber1" 			as "Moneda",
			T1."U_TOTAL_PAGO"			as "Importe",
			T4."Line_ID"				as "NroLineaAsiento",
			'0'							as "NroCuota",
			T1."U_CUENTA_PROV"			as "NroCtaProveedor",
			T1."U_BANCO_PROV"			as "CodBncProveedor",
			T6."WTCode"					as "CodRetencion",
			case when ISNULL(T4."FCCurrency",'') = '' then 1 * (T6."WTAmnt" - T6."ApplAmnt")
			else 1 * (T6."WTAmntFC" - T6."ApplAmntFC") end as "MontoRetencion",
			T4."FCCredit"/(case when T4."Credit" = 0 then 1 else T4."Credit" end) as "TCDocumento",
			(select TX0."Memo" from OJDT TX0 where TX0."TransId" = T4."TransId")	as "GlosaAsiento",
			T1."U_COD_PROV_FACTO"	as "CardCodeFacto",
			T1."U_NOM_PROV_FACTO"	as "CardNameFacto"
		from "@EXD_OEPG" 				T0 
		inner 	join "@EXD_EPG1" 		T1 on T0."DocEntry" 	= T1."DocEntry"
		inner 	join OACT 				T2 on T2."FormatCode" 	= replace(case when T1.U_MONEDA_PAGO = 'SOL' then  T0."U_CTA_BANCO_ML" else T0."U_CTA_BANCO_ME" end,'-','')
		inner 	join DSC1				T3 on T3."GLAccount" 	= T2."AcctCode"
		inner 	join JDT1				T4 on T4."TransId" 		= rtrim(ltrim(T1."U_DOCENTRY")) and T1."U_TIPO_DOCUMENTO" = 'PR'
		inner 	join OCRD				T5 on T5."CardCode"		= T4."ShortName"
		left 	join JDT2				T6 on T4."TransId"		= T6."AbsEntry"								 
		where  T0."U_FECHA_PAGO" = @fechaEscPago and T1."U_NRO_LINEA_AS" = T4."Line_ID" and T5."CardType" = 'C' and T4."DebCred" = 'C' --AND T0."U_ESTADO"='A'
	),
	RSLT2 
	AS
	(
		select T1."ObjType"	,T1."AbsEntry" ,T0."OffclCode" from OWHT T0 inner join PCH5 T1 on T0."WTCode" = T1."WTCode" union all
		select T1."ObjType"	,T1."AbsEntry" ,T0."OffclCode" from OWHT T0 inner join RIN5 T1 on T0."WTCode" = T1."WTCode" union all
		select T1."ObjType" ,T1."AbsEntry" ,T0."OffclCode" from OWHT T0 inner join DPO5 T1 on T0."WTCode" = T1."WTCode" union all
		select T1."ObjType"	,T1."AbsEntry" ,T0."OffclCode" from OWHT T0 inner join JDT2 T1 on T0."WTCode" = T1."WTCode"
	)

	select 
		'Y'							as "SlcPago",
		'N'							as "SlcRetencion",
		T0."CodEscenarioPago",
		T0."NumEscenarioPago",
		T0."DscEscenarioPago",
		T0."MedioDePago",
		T0."MonedaDePago",
		T0."CodBanco",
		T0."CodCtaBanco",
		T0."NumCtaBanco",
		T0."DocEntryDocumento",		
		T0."TipoDocumento",
		T0."NroDocSUNAT",
		T0."EstadoDocumento",
		T0."CardCode",
		T0."CardName",
		T0."Moneda",
		T0."Importe",
		T0."NroCuota",
		T0."NroLineaAsiento",
		T0."NroDocumentoSN",
		T0."NroCtaProveedor",
		T0."CodBncProveedor",
		T0."CodRetencion",
		ISNULL(T0."MontoRetencion",0) as "MontoRetencion",
		ISNULL((select max('Y') from RSLT2 TX0 where ISNULL(TX0."OffclCode",'') = 'RIGV' and TX0."ObjType" = T0."TipoDocumento" 
		and TX0."AbsEntry"= T0."DocEntryDocumento" ),'N') as "AplSerieRetencion",
		T0."TCDocumento",
		T0."GlosaAsiento",
		T0."CardCodeFacto",
		T0."CardNameFacto"
	from RSLT1 T0;
end