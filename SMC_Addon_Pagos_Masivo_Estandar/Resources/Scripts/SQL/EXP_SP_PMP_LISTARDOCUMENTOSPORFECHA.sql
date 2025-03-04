CREATE PROCEDURE EXP_SP_PMP_LISTARDOCUMENTOSPORFECHA --'20231013'
(
	@fechaEscPago date,
	@codSucursal int,
	@codPrioridad varchar(50),
	@tipoDocumento int
)
as
begin

	declare @mndLoc varchar(5) = (select "MainCurncy"  from OADM);
	
	
	--Factura de proveedor
	WITH RSLT1
	as
	(
		 select distinct	
			T4."BPLId"					as "CodSucursal",	
			T0."DocEntry"				as "CodEscenarioPago",
			T0."DocNum"					as "NumEscenarioPago",
			''							as "DscEscenarioPago",
			T0."U_MEDIO_DE_PAGO"		as "MedioDePago",
			T1."U_MONEDA_PAGO"			as "MonedaDePago",
			T3."BankCode" 				as "CodBanco",
			T2."AcctCode" 				as "CodCtaBanco",
			T1."U_NRO_CTA_PAGO" 		as "NumCtaBanco",
			T1."U_DOCENTRY"				as "DocEntryDocumento",
			T4."ObjType"				as "TipoDocumento",
			T4."NumAtCard" 				as "NroDocSUNAT",
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
			T1."U_RETENCION"/*case when T4."DocCur" = @mndLoc then 1 * (T7."WTAmnt" - T7."ApplAmnt")
			else 1 * (T7."WTAmntFC" - T7."ApplAmntFC") end*/ as "MontoRetencion",
			T4."DocRate"				as "TCDocumento",
			T4."JrnlMemo"				as "GlosaAsiento",
			T1."U_COD_PROV_FACTO"		as "CardCodeFacto",
			T1."U_NOM_PROV_FACTO"		as "CardNameFacto",
			T1."U_AFECTO_RETENCION"		as "AfectoRetencion",
			T1."U_TIENE_RETENCION"		as "TieneRetencion",
			T1."U_APLICA_RETENCION"		as "AplicaRetencion",
			T4."U_EXX_PRIPAG"			as "CodPrioridad",
			T1."LineId"					as "NroLineaEP"
		from "@EXD_OEPG" 				T0 
		inner 	join "@EXD_EPG1" 		T1 on T0."DocEntry" 	= T1."DocEntry"
		inner 	join OACT 				T2 on T2."AcctCode" 	= T1.U_COD_CTA_PAGO
		inner 	join DSC1				T3 on T3."GLAccount" 	= T2."AcctCode"
		inner 	join OPCH				T4 on T4."DocEntry" 	= rtrim(ltrim(T1."U_DOCENTRY")) and T1."U_TIPO_DOCUMENTO" = 'FT-P'
		inner 	join PCH6 				T5 on T5."DocEntry"		= T4."DocEntry" AND T5."InstlmntID"=T1."U_NRO_CUOTA"
		inner 	join OCRD				T6 on T4."CardCode"		= T6."CardCode"
		left 	join PCH5				T7 on T4."DocEntry"		= T7. "AbsEntry"
		where  T0."U_FECHA_PAGO" = @fechaEscPago and isnull(T4."DocStatus",'') != 'C' and isnull(T4."CANCELED",'') = 'N' 
		and T5."Status" = 'O' and T0."U_ESTADO"='A' 

		union all 
		--Nota de credito de cliente
		select distinct		
			T4."BPLId"					as "CodSucursal",
			T0."DocEntry"				as "CodEscenarioPago",
			T0."DocNum"					as "NumEscenarioPago",
			''							as "DscEscenarioPago",
			T0."U_MEDIO_DE_PAGO"		as "MedioDePago",
			T1."U_MONEDA_PAGO"			as "MonedaDePago",
			T3."BankCode" 				as "CodBanco",
			T2."AcctCode" 				as "CodCtaBanco",
			T1."U_NRO_CTA_PAGO" 		as "NumCtaBanco",
			T1."U_DOCENTRY"				as "DocEntryDocumento",
			T4."ObjType"				as "TipoDocumento",
			T4."NumAtCard" 				as "NroDocSUNAT",
			T4."DocStatus"				as "EstadoDocumento",
			T4."CardCode"				as "CardCode",
			T4."CardName"				as "CardName",
			T5."LicTradNum"				as "NroDocumentoSN",
			T4."DocCur" 				as "Moneda",
			T1."U_TOTAL_PAGO"			as "Importe",
			'0'							as "NroLineaAsiento",
			'0'							as "NroCuota",
			T1."U_CUENTA_PROV"			as "NroCtaProveedor",
			T1."U_COD_BANCO"			as "CodBncProveedor",
			T6."WTCode"					as "CodRetencion",
			case when T4."DocCur" = @mndLoc then 1 * (T6."WTAmnt" - T6."ApplAmnt") 
			else 1 * (T6."WTAmntFC" - T6."ApplAmntFC") end as "MontoRetencion",
			T4."DocRate"				as "TCDocumento",
			T4."JrnlMemo"				as "GlosaAsiento",
			T1."U_COD_PROV_FACTO"		as "CardCodeFacto",
			T1."U_NOM_PROV_FACTO"		as "CardNameFacto",
			T1."U_AFECTO_RETENCION"		as "AfectoRetencion",
			T1."U_TIENE_RETENCION"		as "TieneRetencion",
			T1."U_APLICA_RETENCION"		as "AplicaRetencion",
			T4."U_EXX_PRIPAG"			as "CodPrioridad",
			T1."LineId"					as "NroLineaEP"
		from "@EXD_OEPG" 				T0 
		inner 	join "@EXD_EPG1" 		T1 on T0."DocEntry" 	= T1."DocEntry"
		inner 	join OACT 				T2 on T2."AcctCode" 	= T1.U_COD_CTA_PAGO
		inner 	join DSC1				T3 on T3."GLAccount" 	= T2."AcctCode"
		inner  	join ORIN				T4 on T4."DocEntry" 	= rtrim(ltrim(T1."U_DOCENTRY")) and T1."U_TIPO_DOCUMENTO" = 'NC-C'
		inner 	join OCRD				T5 on T4."CardCode"		= T5."CardCode"
		left 	join RIN5				t6 on T6."AbsEntry"		= T4."DocEntry" 
		where  T0."U_FECHA_PAGO" = @fechaEscPago and T4."DocStatus" = 'O' AND T0."U_ESTADO" = 'A'
	
		union all
		--Anticipo de proveedor
		select distinct		
			T4."BPLId"					as "CodSucursal",
			T0."DocEntry"				as "CodEscenarioPago",
			T0."DocNum"					as "NumEscenarioPago",
			''							as "DscEscenarioPago",
			T0."U_MEDIO_DE_PAGO"		as "MedioDePago",
			T1."U_MONEDA_PAGO"			as "MonedaDePago",
			T3."BankCode" 				as "CodBanco",
			T2."AcctCode" 				as "CodCtaBanco",
			T1."U_NRO_CTA_PAGO" 		as "NumCtaBanco",
			T1."U_DOCENTRY"				as "DocEntryDocumento",
			T4."ObjType"				as "TipoDocumento",
			T4."NumAtCard" 				as "NroDocSUNAT",
			T4."DocStatus"				as "EstadoDocumento",
			T4."CardCode"				as "CardCode",
			T4."CardName"				as "CardName",
			T5."LicTradNum"				as "NroDocumentoSN",
			T4."DocCur" 				as "Moneda",
			T1."U_TOTAL_PAGO"			as "Importe",
			'0'							as "NroLineaAsiento",
			T1."U_NRO_CUOTA"			as "NroCuota",
			T1."U_CUENTA_PROV"			as "NroCtaProveedor",
			T1."U_COD_BANCO"			as "CodBncProveedor",
			''							as "CodRetencion",
			0 							as "MontoRetencion",
			T4."DocRate"				as "TCDocumento",
			T4."JrnlMemo"				as "GlosaAsiento",
			T1."U_COD_PROV_FACTO"		as "CardCodeFacto",
			T1."U_NOM_PROV_FACTO"		as "CardNameFacto",
			T1."U_AFECTO_RETENCION"		as "AfectoRetencion",
			T1."U_TIENE_RETENCION"		as "TieneRetencion",
			T1."U_APLICA_RETENCION"		as "AplicaRetencion",
			T4."U_EXX_PRIPAG"			as "CodPrioridad",
			T1."LineId"					as "NroLineaEP"
		from "@EXD_OEPG" 				T0 
		inner 	join "@EXD_EPG1" 		T1 on T0."DocEntry" 	= T1."DocEntry"
		inner 	join OACT 				T2 on T2."AcctCode" 	= T1.U_COD_CTA_PAGO
		inner 	join DSC1				T3 on T3."GLAccount" 	= T2."AcctCode"
		inner 	join ODPO				T4 on T4."DocEntry" 	= rtrim(ltrim(T1."U_DOCENTRY")) and T1."U_TIPO_DOCUMENTO" IN ('FA-P','SA-P')
		inner 	join OCRD				T5 on T4."CardCode"		= T5."CardCode"
		--inner 	join DPO5				T6 on T6."AbsEntry"		= T4."DocEntry"
		where  T0."U_FECHA_PAGO" = @fechaEscPago and T4."DocStatus" = 'O' AND T0."U_ESTADO"='A'
	
		union all
		--Pago borrador
		select distinct	
			T4."BPLId"					as "CodSucursal",	
			T0."DocEntry"				as "CodEscenarioPago",
			T0."DocNum"					as "NumEscenarioPago",
			''							as "DscEscenarioPago",
			T0."U_MEDIO_DE_PAGO"		as "MedioDePago",
			T1."U_MONEDA_PAGO"			as "MonedaDePago",
			T3."BankCode" 				as "CodBanco",
			T2."AcctCode" 				as "CodCtaBanco",
			T1."U_NRO_CTA_PAGO" 		as "NumCtaBanco",
			T1."U_DOCENTRY"				as "DocEntryDocumento",
			'140'						as "TipoDocumento",
			T4."U_EXX_NUMEREND" 		as "NroDocSUNAT",
			'O'							as "EstadoDocumento",
			T4."CardCode"				as "CardCode",
			T4."CardName"				as "CardName",
			T5."LicTradNum"				as "NroDocumentoSN",
			T4."DocCurr" 				as "Moneda",
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
			T1."U_NOM_PROV_FACTO"		as "CardNameFacto",
			T1."U_AFECTO_RETENCION"		as "AfectoRetencion",
			T1."U_TIENE_RETENCION"		as "TieneRetencion",
			T1."U_APLICA_RETENCION"		as "AplicaRetencion",
			T4."U_EXX_PRIPAG"			as "CodPrioridad",
			T1."LineId"					as "NroLineaEP"
		from "@EXD_OEPG" 				T0 
		inner 	join "@EXD_EPG1" 		T1 on T0."DocEntry" 	= T1."DocEntry"
		inner 	join OACT 				T2 on T2."AcctCode" 	= T1.U_COD_CTA_PAGO
		inner 	join DSC1				T3 on T3."GLAccount" 	= T2."AcctCode"
		inner 	join OPDF				T4 on T4."DocEntry" 	= rtrim(ltrim(T1."U_DOCENTRY")) and T1."U_TIPO_DOCUMENTO" = 'SP'
		inner 	join OCRD				T5 on T4."CardCode"		= T5."CardCode"
		where  T0."U_FECHA_PAGO" = @fechaEscPago and T4."ObjType" = '46' and T4."Canceled" = 'N' AND T0."U_ESTADO"='A'
	
		union all
		--Asiento
		select distinct		
			T4."BPLId"					as "CodSucursal",
			T0."DocEntry"				as "CodEscenarioPago",
			T0."DocNum"					as "NumEscenarioPago",
			''							as "DscEscenarioPago",
			T0."U_MEDIO_DE_PAGO"		as "MedioDePago",
			T1."U_MONEDA_PAGO"			as "MonedaDePago",
			T3."BankCode" 				as "CodBanco",
			T2."AcctCode" 				as "CodCtaBanco",
			T1."U_NRO_CTA_PAGO" 		as "NumCtaBanco",
			T1."U_DOCENTRY"				as "DocEntryDocumento",
			T4."ObjType"				as "TipoDocumento",
			T4."Ref2" 					as "NroDocSUNAT",
			'O'							as "EstadoDocumento",
			T4."ShortName"				as "CardCode",
			T5."CardName"				as "CardName",
			T5."LicTradNum"				as "NroDocumentoSN",
			isnull(T4."FCCurrency",'SOL')			as "Moneda",
			T1."U_TOTAL_PAGO"			as "Importe",
			T4."Line_ID"				as "NroLineaAsiento",
			'0'							as "NroCuota",
			T1."U_CUENTA_PROV"			as "NroCtaProveedor",
			T1."U_COD_BANCO"			as "CodBncProveedor",
			T6."WTCode"					as "CodRetencion",
			case when isnull(T4."FCCurrency",'') = '' then 1 * (T6."WTAmnt" - T6."ApplAmnt") 
			else 1 * (T6."WTAmntFC" - T6."ApplAmntFC") end as "MontoRetencion",
			T4."FCCredit"/(case when T4."Credit" = 0 then 1 else T4."Credit" end) as "TCDocumento",
			(select TX0."Memo" from OJDT TX0 where TX0."TransId" = T4."TransId")	as "GlosaAsiento",
			T1."U_COD_PROV_FACTO"		as "CardCodeFacto",
			T1."U_NOM_PROV_FACTO"		as "CardNameFacto",
			T1."U_AFECTO_RETENCION"		as "AfectoRetencion",
			T1."U_TIENE_RETENCION"		as "TieneRetencion",
			T1."U_APLICA_RETENCION"		as "AplicaRetencion",
			T7."U_EXX_PRIPAG"			as "CodPrioridad",
			T1."LineId"					as "NroLineaEP"
		from "@EXD_OEPG" 				T0 
		inner 	join "@EXD_EPG1" 		T1 on T0."DocEntry" 	= T1."DocEntry"
		inner 	join OACT 				T2 on T2."AcctCode" 	= T1.U_COD_CTA_PAGO
		inner 	join DSC1				T3 on T3."GLAccount" 	= T2."AcctCode"
		inner 	join JDT1				T4 on T4."TransId" 		= rtrim(ltrim(T1."U_DOCENTRY")) and T1."U_TIPO_DOCUMENTO" = 'AS'
		inner 	join OCRD				T5 on T5."CardCode"		= T4."ShortName"
		inner 	join OJDT				T7 on T4."TransId"		= T7."TransId"	
		left 	join JDT2				T6 on T4."TransId"		= T6."AbsEntry"						 
		where  T0."U_FECHA_PAGO" = 		@fechaEscPago 
		and T1."U_NRO_LINEA_AS" = T4."Line_ID" 
		and /*T5."CardType" = 'S' and*/ T4."DebCred" = 'C' AND T0."U_ESTADO" = 'A'
		and (select isnull(max(TX0."U_EXP_ESTADO"),'') from "@EXP_PMP1" TX0 
		--inner join OVPM TX1 on TX0."U_EXP_NROPGOEFEC" = TO_VARCHAR(TX1."DocEntry") 
		where TX0."U_EXP_TIPODOC" = T4."ObjType" and TX0."U_EXP_DOCENTRYDOC" = T4."TransId" 
		and TX0."U_EXP_ASNROLINEA" = T4."Line_ID" /*and isnull(TX1."Canceled",'') = 'N'*/
		and TX0."U_EXP_COD_ESCENARIOPAGO" = T0."DocEntry") != 'OK'
	
		union all  
		--Pago recibido
		select DISTINCT 	
			T4."BPLId"					as "CodSucursal",	
			T0."DocEntry"				as "CodEscenarioPago",
			T0."DocNum"					as "NumEscenarioPago",
			''							as "DscEscenarioPago",
			T0."U_MEDIO_DE_PAGO"		as "MedioDePago",
			T1."U_MONEDA_PAGO"			as "MonedaDePago",
			T3."BankCode" 				as "CodBanco",
			T2."AcctCode" 				as "CodCtaBanco",
			T1."U_NRO_CTA_PAGO" 		as "NumCtaBanco",
			T1."U_DOCENTRY"				as "DocEntryDocumento",
			'24'						as "TipoDocumento",
			(select max(TX0."U_EXX_NUMEREND") from ORCT TX0 where TX0."TransId" = T4."TransId")	as "NroDocSUNAT",
			'O'							as "EstadoDocumento",
			T4."ShortName"				as "CardCode",
			T5."CardName"				as "CardName",
			T5."LicTradNum"				as "NroDocumentoSN",
			isnull(T4."FCCurrency",'SOL') 			as "Moneda",
			T1."U_TOTAL_PAGO"			as "Importe",
			T4."Line_ID"				as "NroLineaAsiento",
			'0'							as "NroCuota",
			T1."U_CUENTA_PROV"			as "NroCtaProveedor",
			T1."U_COD_BANCO"			as "CodBncProveedor",
			T6."WTCode"					as "CodRetencion",
			case when isnull(T4."FCCurrency",'') = '' then 1 * (T6."WTAmnt" - T6."ApplAmnt")
			else 1 * (T6."WTAmntFC" - T6."ApplAmntFC") end as "MontoRetencion",
			T4."FCCredit"/(case when T4."Credit" = 0 then 1 else T4."Credit" end) as "TCDocumento",
			(select TX0."Memo" from OJDT TX0 where TX0."TransId" = T4."TransId")	as "GlosaAsiento",
			T1."U_COD_PROV_FACTO"	as "CardCodeFacto",
			T1."U_NOM_PROV_FACTO"	as "CardNameFacto",
			T1."U_AFECTO_RETENCION"		as "AfectoRetencion",
			T1."U_TIENE_RETENCION"		as "TieneRetencion",
			T1."U_APLICA_RETENCION"		as "AplicaRetencion",
			T7."U_EXX_PRIPAG"			as "CodPrioridad",
			T1."LineId"					as "NroLineaEP"
		from "@EXD_OEPG" 				T0 
		inner 	join "@EXD_EPG1" 		T1 on T0."DocEntry" 	= T1."DocEntry"
		inner 	join OACT 				T2 on T2."AcctCode" 	= T1.U_COD_CTA_PAGO
		inner 	join DSC1				T3 on T3."GLAccount" 	= T2."AcctCode"
		inner 	join JDT1				T4 on T4."TransId" 		= rtrim(ltrim(T1."U_DOCENTRY")) and T1."U_TIPO_DOCUMENTO" = 'PR'
		inner 	join OCRD				T5 on T5."CardCode"		= T4."ShortName"
		inner  	join OJDT				T7 on T4."TransId"		= T7."TransId"
		left 	join JDT2				T6 on T4."TransId"		= T6."AbsEntry"								 
		where  T0."U_FECHA_PAGO" = @fechaEscPago and T1."U_NRO_LINEA_AS" = T4."Line_ID" /*and T5."CardType" = 'C'*/ and T4."DebCred" = 'C' AND T0."U_ESTADO"='A'
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
		T0."CodSucursal",
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
		T0."Importe" - case when T0."AplicaRetencion" = 'Y' then isnull(T0."MontoRetencion",0) else 0.00 end as "Importe",
		T0."NroCuota",
		T0."NroLineaAsiento",
		T0."NroDocumentoSN",
		T0."NroCtaProveedor",
		T0."CodBncProveedor",
		case when isnull(T0."MontoRetencion",0) > 0 then T0."CodRetencion" else '' end "CodRetencion",
		isnull(T0."MontoRetencion",0) as "MontoRetencion",
		case when isnull(T0."MontoRetencion",0) > 0 then  isnull((select max('Y') from RSLT2 TX0 where isnull(TX0."OffclCode",'') = 'RIGV' and TX0."ObjType" = T0."TipoDocumento" 
		and TX0."AbsEntry"= T0."DocEntryDocumento" ),'N') else 'N' end as "AplSerieRetencion",
		T0."TCDocumento",
		T0."GlosaAsiento",
		T0."CardCodeFacto",
		T0."CardNameFacto",
		T0."AfectoRetencion",
		T0."TieneRetencion",
		T0."AplicaRetencion",
		T0."CodPrioridad",
		T0."NroLineaEP"
	from RSLT1 T0 
	where (isnull((select max('Y') from "@EXP_PMP1" TX0 
	where TX0."U_EXP_COD_ESCENARIOPAGO" = T0."CodEscenarioPago" 
	and TX0."U_EXP_TIPODOC" = T0."TipoDocumento"
	and TX0."U_EXP_DOCENTRYDOC" = T0."DocEntryDocumento"
	and isnull(TX0."U_EXP_ASNROLINEA",'0') = isnull(T0."NroLineaAsiento",'0')
	and isnull(TX0."U_EXP_NMROCUOTA",'0') = isnull(T0."NroCuota",'0')),'') != 'Y')
	and isnull(T0."codSucursal",'0') = isnull(case when @codSucursal = '-1' then T0."CodSucursal" else @codSucursal end,'0')
	and isnull(T0."CodPrioridad",'') = isnull(case when @codPrioridad = '' then T0."CodPrioridad" else @codPrioridad end,'')
	and T0."TipoDocumento" = case when @tipoDocumento = '0' then T0."TipoDocumento" else @tipoDocumento end;
end