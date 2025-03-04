CREATE PROCEDURE SMC_APM_LISTAR_FACPENDIENTES_PP
(
	fechaVencD date,
	fechaVencH date,
	monedaLoc VARCHAR(3),
	monedaExt VARCHAR(3),
	CardCode VARCHAR(20),
	escenario varchar(15),
	tipoBanco varchar(3),
	filtroBanco varchar(3),
	codSucursal varchar(3),
	codPrioridad varchar(50),
	codTipoDocumento varchar(5),
	montoMinimo decimal(16,8)
)
AS
BEGIN
	declare tipoCambioP decimal(18,6);
	declare montoMinimoRET decimal(19,6);
	
	select ifnull(max("Rate"),1) into tipoCambioP from ORTT where "Currency" = 'USD' and "RateDate" = TO_DATE(now());
	select ifnull(max(U_EXX_MONTOMIN),0.00) into montoMinimoRET from OWHT where "WTCode" = 'RIGV';
	
	PAG_PAR = select 
		sum(case when ifnull(U_COD_RETENCION,'') <> '' and ifnull(U_TOTAL,0) <> ifnull(U_TOTAL_PAGO,0) then ifnull(U_TOTAL_PAGO,0) + ifnull(U_RETENCION,0) else ifnull(U_TOTAL_PAGO,0) end) as "MONTO" 
		,U_TIPO_DOCUMENTO,U_DOCENTRY,U_NRO_CUOTA,U_NRO_LINEA_AS 
	from 
	(
	select 
		U_TOTAL,U_TOTAL_PAGO,U_RETENCION,U_TIPO_DOCUMENTO,U_DOCENTRY,U_NRO_CUOTA,U_NRO_LINEA_AS,U_COD_RETENCION
	from "@EXD_OEPG" T0 inner join "@EXD_EPG1" T1 on T0."DocEntry" = T1."DocEntry"
	where ifnull("Canceled",'') <> 'Y'	
	and ifnull((select max('Y') from "@EXP_PMP1" TX0 inner join "@EXP_OPMP" TX1 on TX0."DocEntry" = TX1."DocEntry"
	where T0."DocEntry" = TX0.U_EXP_COD_ESCENARIOPAGO
	and T1.U_NRO_CUOTA 		= TX0.U_EXP_NMROCUOTA 
	and T1.U_NRO_LINEA_AS 	= TX0.U_EXP_ASNROLINEA 
	and T1.U_DOCENTRY 		= TX0.U_EXP_DOCENTRYDOC 
	and (case T1.U_TIPO_DOCUMENTO 
		when 'FT-P' then 18 end) = TX0.U_EXP_TIPODOC
	/*and TX1."Status" = 'C' and ifnull(TX0.U_EXP_ESTADO,'') IN ('','OK')*/ 
	),'') <> 'Y'
	
	union all 
	
	select 
		U_TOTAL,U_TOTAL_PAGO,U_RETENCION,U_TIPO_DOCUMENTO,U_DOCENTRY,U_NRO_CUOTA,U_NRO_LINEA_AS,U_COD_RETENCION
	from "@EXD_OEPG" T0 inner join "@EXD_EPG1" T1 on T0."DocEntry" = T1."DocEntry"
	where ifnull("Canceled",'') <> 'Y'	
	and ifnull((select max('Y') from "@EXP_PMP1" TX0 inner join "@EXP_OPMP" TX1 on TX0."DocEntry" = TX1."DocEntry"
	where T0."DocEntry" = TX0.U_EXP_COD_ESCENARIOPAGO
	and T1.U_NRO_CUOTA 		= TX0.U_EXP_NMROCUOTA 
	and T1.U_NRO_LINEA_AS 	= TX0.U_EXP_ASNROLINEA 
	and T1.U_DOCENTRY 		= TX0.U_EXP_DOCENTRYDOC 
	and (case T1.U_TIPO_DOCUMENTO 
		when 'FT-P' then 18 end) = TX0.U_EXP_TIPODOC
	and ifnull(TX0.U_EXP_ESTADO,'') <> 'OK'
	/*and TX1."Status" = 'C'and ifnull(TX0.U_EXP_ESTADO,'') IN ('','OK')*/  
	),'') = 'Y'
	)AS TT  
	group by U_TIPO_DOCUMENTO,U_DOCENTRY,U_NRO_CUOTA,U_NRO_LINEA_AS;
	
	DOCS = SELECT 
		'N' as "Slc",
		T0."CodSucursal",
		ROW_NUMBER() OVER(ORDER BY T0."FechaVencimiento" DESC) AS "FILA",
		T0."DocEntry",
		T0."DocNum",
		T0."FechaContable",
		T0."FechaVencimiento",
		T0."CardCode",
		T0."CardName",
		T0."NumAtCard",
		(select max(TX0."BankCode") from ODSC TX0 inner join DSC1 TX1 on TX0."BankCode" = TX1."BankCode" where TX0."BankCode" = case when :tipoBanco = '000' then T0."BankCode" else  :tipoBanco end and TX1."Branch" = T0."CodSucursal" and TX1."UsrNumber4" = T0."DocCur" and ifnull(T0."BankCode",'') != '') as "CodBancoPago",
		(select max(TX0."BankName") from ODSC TX0 inner join DSC1 TX1 on TX0."BankCode" = TX1."BankCode" where TX0."BankCode" = case when :tipoBanco = '000' then T0."BankCode" else  :tipoBanco end and TX1."Branch" = T0."CodSucursal" and TX1."UsrNumber4" = T0."DocCur" and ifnull(T0."BankCode",'') != '') as "NomBancoPago",
		--(select TX0."BankCode" from ODSC TX0 inner join DSC1 TX1 on TX0."BankCode" = TX1."BankCode" where TX0."BankCode" = T0."BankCode" and TX1."Branch" = T0."CodSucursal" and TX1."UsrNumber4" = T0."DocCur")	as "CodBancoPago",
		--(select TX0."BankName" from ODSC TX0 inner join DSC1 TX1 on TX0."BankCode" = TX1."BankCode" where TX0."BankCode" = T0."BankCode" and TX1."Branch" = T0."CodSucursal" and TX1."UsrNumber4" = T0."DocCur")	as "NomBancoPago",
		--case when :tipoBanco = '000' then T0."BankCode" else case when ifnull(T0."BankCode",'') != '' then :tipoBanco end end as "CodBancoPago",
		T0."DocCur" as "MonedaPago",
		(select (select I0."GLAccount" from DSC1 I0 where I0."AbsEntry" = MAX(TX0."AbsEntry")) from DSC1 TX0 
			inner join OACT TX1 on TX0."GLAccount" = TX1."AcctCode" 			
			where TX0."BankCode" = case when :tipoBanco = '000' then T0."BankCode" else :tipoBanco end 			
			and TX1."ActCurr" = T0."DocCur" 
			and TX0."Branch" = T0."CodSucursal" 
			and ifnull(T0."BankCode",'') != '' 
			and ifnull(TX0."U_EXM_PMASIVO",'') = 'Y') as "CodCtaPago",
		(select (select I0."Account" from DSC1 I0 where I0."AbsEntry" = MAX(TX0."AbsEntry")) from DSC1 TX0 
			inner join OACT TX1 on TX0."GLAccount" = TX1."AcctCode" 
			where TX0."BankCode" = case when :tipoBanco = '000' then T0."BankCode" else :tipoBanco end 			
			and TX1."ActCurr" = T0."DocCur" 
			and TX0."Branch" = T0."CodSucursal" 
			and ifnull(T0."BankCode",'') != '' 
			and ifnull(TX0."U_EXM_PMASIVO",'') = 'Y') as "NroCtaPago",
		--(select TX0."Account" from DSC1 TX0 inner join OACT TX1 on TX0."GLAccount" = TX1."AcctCode" where TX0."BankCode" = case when :tipoBanco = '000' then T0."BankCode" else :tipoBanco end and TX1."ActCurr" = T0."DocCur" and TX0."Branch" = T0."CodSucursal" and ifnull(T0."BankCode",'') != '') as "NroCtaPago",
		T0."DocCur",
		T0."Total",
		case when T0."Retencion" > 0 then T0."CodigoRetencion" else '' end as "CodigoRetencion",
		T0."Retencion",
		
		T0."TotalPagar" - ifnull((select MONTO from :PAG_PAR TX0 where TX0.U_TIPO_DOCUMENTO = T0."Documento"
		and TX0.U_DOCENTRY = T0."DocEntry" and TX0.U_NRO_CUOTA = T0."NroCuota" and TX0.U_NRO_LINEA_AS = T0."LineaAsiento"),0) as "Saldo",
		
		T0."TotalPagar" - ifnull((select MONTO from :PAG_PAR TX0 where TX0.U_TIPO_DOCUMENTO = T0."Documento"
		and TX0.U_DOCENTRY = T0."DocEntry" and TX0.U_NRO_CUOTA = T0."NroCuota" and TX0.U_NRO_LINEA_AS = T0."LineaAsiento"),0) as "TotalPagar",

		T0."RUC",
		T0."Cuenta",
		T0."CuentaMoneda",
		T0."BankCode",
		T0."Atraso",
		T0."Marca",
		T0."Estado",
		T0."Documento",
		(select MAX("BankName")as "BankName" from ODSC where "BankCode" = T0."BankCode") as "NombreBanco",
		T0."Origen",
		
		/*
		T0."Comentario",
		T0."Propiedad",
		T0."DueDate",
		*/
		T0."BloqueoPago",
		IFNULL(T0."DetraccionPend", 'N') AS "DetraccionPend"
		,T0."NroCuota"
		,T0."LineaAsiento"
		,T0."GlosaAsiento"
		,T0."CardCodeFactoring" 
		,T0."CardNameFactoring" 
		,T0."CodPrioridad"
		,case when ifnull(T0."CodigoRetencion",'') = 'RIGV'							then 'Y' else 'N' end as "AfectoRetencion"
		,case when ifnull(T0."CodigoRetencion",'') = 'RIGV' and T0."Retencion" > 0	then 'Y' else 'N' end as "TieneRetencion"
		,case when ifnull(T0."CodigoRetencion",'') = 'RIGV' and ("Total" * "DocRate") > :montoMinimoRET	then 'Y' else 'N' end as "AplicaRetencion"
	FROM 
		(SELECT
		T0."DocEntry",
		T0."DocNum",
		T0."TaxDate" AS "FechaContable",
		T1."DueDate" AS "FechaVencimiento",
		T0."CardCode",
		T0."CardName",
		T0."NumAtCard",
		T0."DocCur",
		T0."DocRate",
		CAST( 
		(CASE 
			WHEN T0."DocCur" in ('USD','EUR') 
				THEN T1."InsTotalFC" --- T1."PaidFC" + 
				--(SELECT (R0."InsTotalFC"- R0."PaidFC") FROM PCH6 R0 WHERE R0."DocEntry" = T0."DocEntry" AND R0."InstlmntID" = '1')
			ELSE 
				T1."InsTotal" --- T1."PaidToDate" --+ 
				--(SELECT (R0."InsTotal"- R0."PaidToDate") FROM PCH6 R0 WHERE R0."DocEntry" = T0."DocEntry" AND R0."InstlmntID" = '1')
		END)AS DECIMAL(16,2)) AS "Total",
		
		IFNULL(T5."OffclCode",'') AS "CodigoRetencion" ,
		
		CAST((CASE 
			WHEN T0."DocCur" in ('USD','EUR') THEN 
				T1."WTSumFC"
			ELSE 
				T1."WTSum"
		END)AS DECIMAL(16,2)) AS "Retencion",
		
		
		/*
		CAST((CASE 	WHEN T0."DocCur" in ('USD','EUR') THEN 
							CASE (SELECT "WTCode" FROM PCH5 WHERE "AbsEntry" = T0."DocEntry")
								WHEN 'RT4C' THEN (T1."InsTotalFC" - T1."PaidFC") - ((T1."InsTotalFC"/(T1."InsTotalFC" - T1."PaidFC")) * (T4."WTAmntFC" - T4."ApplAmntFC"))
								WHEN 'RIGV' THEN (T1."InsTotalFC" - T1."PaidFC") - ((T1."InsTotalFC"/(T1."InsTotalFC" - T1."PaidFC")) * (T4."WTAmntFC" - T4."ApplAmntFC"))
								ELSE T1."InsTotalFC" - T1."PaidFC"--+ T0."WTSumFC" - T4."ApplAmntFC"
							END
						ELSE 
							CASE (SELECT "WTCode" FROM PCH5 WHERE "AbsEntry" = T0."DocEntry")
								WHEN 'RT4C' THEN(T1."InsTotal" - T1."PaidToDate") - ((T1."InsTotal" /(T1."InsTotal" - T1."PaidToDate")) * (T4."WTAmnt" - T4."ApplAmnt")) 
								WHEN 'RIGV' THEN  (T1."InsTotal" - T1."PaidToDate") - ((T1."InsTotal" /(T1."InsTotal" - T1."PaidToDate")) * (T4."WTAmnt" - T4."ApplAmnt")) 
								ELSE T1."InsTotal" - T1."PaidToDate"-- + T0."WTSum" - T4."ApplAmnt"
							END
			END)AS DECIMAL(16,2)) AS "TotalPagar",
		*/	
		
		
		CAST((CASE 	WHEN T0."DocCur" in ('USD','EUR') THEN 
							CASE (SELECT "WTCode" FROM PCH5 WHERE "AbsEntry" = T0."DocEntry")
								--WHEN 'RT4C' THEN (T1."InsTotalFC" - T1."PaidFC") - ((T1."InsTotalFC"/(T1."InsTotalFC" - T1."PaidFC")) * (T4."WTAmntFC" - T4."ApplAmntFC"))
								WHEN 'RIGV' THEN (T1."InsTotalFC"/*-T1."WTSumFC"*/)-(T1."PaidFC"/*-T1."WTAppliedF"*/)
								ELSE T1."InsTotalFC" - T1."PaidFC"--+ T0."WTSumFC" - T4."ApplAmntFC"
							END
						ELSE 
							CASE (SELECT "WTCode" FROM PCH5 WHERE "AbsEntry" = T0."DocEntry")
								--WHEN 'RT4C' THEN(T1."InsTotal" - T1."PaidToDate") - ((T1."InsTotal" /(T1."InsTotal" - T1."PaidToDate")) * (T4."WTAmnt" - T4."ApplAmnt")) 
								WHEN 'RIGV' THEN  (T1."InsTotal"/*-T1."WTSum"*/)-(T1."PaidToDate"/*-T1."WTApplied"*/) 
								ELSE T1."InsTotal" - T1."PaidToDate"-- + T0."WTSum" - T4."ApplAmnt"
							END
			END)AS DECIMAL(16,2)) AS "TotalPagar",
			--(T1."InsTotal"-T4."WTAmnt")-(T1."PaidToDate"-T4."ApplAmnt"),
			
		T3."LicTradNum" AS "RUC",
		
		
		
		/*
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."Account") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" --*AND "BankCode" = :tipoBanco
			AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,'')*/
		(SELECT CASE T0."U_EXC_PAGBEN"
			WHEN 'Y' THEN
			CASE  R2."BankCode" WHEN R2."BankCode" THEN R2."Account" ELSE R2."U_EXM_INTERBANCARIA" END
			ELSE CASE :tipoBanco WHEN '000' THEN R3."Account" ELSE CASE WHEN :tipoBanco = R3."BankCode" THEN R3."Account" ELSE R3."U_EXM_INTERBANCARIA" END END END
		FROM OCRD R0
		LEFT JOIN OCPR R1 ON R0."CardCode"=R1."CardCode" AND R1."U_EXC_BENEFI"='Y'
		LEFT JOIN OCRB R2 ON R0."CardCode"=R2."CardCode" AND R1."Name"=R2."U_EXC_BENEFI" AND IFNULL(R2."UsrNumber1",'') = T0."DocCur" --:monedaLoc --beneficiario
		LEFT JOIN OCRB R3 ON R0."CardCode"=R3."CardCode" AND R3."U_EXC_ACTIVO"='Y' AND IFNULL(R3."UsrNumber1",'') = T0."DocCur" --:monedaLoc --normal
		WHERE R0."CardCode" = T0."CardCode")
		AS "Cuenta",
		
		/*
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" --AND "BankCode" = :tipoBanco
			AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,'') */
		(SELECT CASE T0."U_EXC_PAGBEN" WHEN 'Y' THEN R2."UsrNumber1" ELSE R3."UsrNumber1" END
		FROM OCRD R0
		LEFT JOIN OCPR R1 ON R0."CardCode"=R1."CardCode" AND R1."U_EXC_BENEFI"='Y'
		LEFT JOIN OCRB R2 ON R0."CardCode"=R2."CardCode" AND R1."Name"=R2."U_EXC_BENEFI" AND IFNULL(R2."UsrNumber1",'') = T0."DocCur"--:monedaLoc  --beneficiario
		LEFT JOIN OCRB R3 ON R0."CardCode"=R3."CardCode" AND R3."U_EXC_ACTIVO"='Y' AND IFNULL(R3."UsrNumber1",'') = T0."DocCur"--:monedaLoc --normal
		WHERE R0."CardCode" = T0."CardCode")
		AS "CuentaMoneda",
		
		/*
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" --AND "BankCode" = :tipoBanco
			AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,'')*/
		(SELECT CASE T0."U_EXC_PAGBEN" WHEN 'Y' THEN R2."BankCode" ELSE R3."BankCode" END
		FROM OCRD R0
		LEFT JOIN OCPR R1 ON R0."CardCode"=R1."CardCode" AND R1."U_EXC_BENEFI"='Y'
		LEFT JOIN OCRB R2 ON R0."CardCode"=R2."CardCode" AND R1."Name"=R2."U_EXC_BENEFI" AND IFNULL(R2."UsrNumber1",'') = T0."DocCur"--:monedaLoc   --beneficiario
		LEFT JOIN OCRB R3 ON R0."CardCode"=R3."CardCode" AND R3."U_EXC_ACTIVO"='Y' AND IFNULL(R3."UsrNumber1",'') = T0."DocCur"--:monedaLoc --normal
		WHERE R0."CardCode" = T0."CardCode")
		AS "BankCode",
		
		(CASE 
			WHEN DAYS_BETWEEN(T0."DocDueDate", :fechaVencH) < 0 THEN 0 
			ELSE DAYS_BETWEEN(T0."DocDueDate", :fechaVencH) 
		END) AS "Atraso",
		--0 AS "Atraso",
		'' AS "Marca",
		'' AS "Estado",
		'FT-P' as "Documento",
		T0."Comments" AS "Comentario",
		CASE WHEN T3."QryGroup11" = 'Y' THEN 'PROV. CAJA CHICA' ELSE '' END AS "Propiedad",
		T1."DueDate"
		,IFNULL(T0."U_EXC_ORIGEN",'') as "Origen"
		,IFNULL(T0."PayBlock", 'N') AS "BloqueoPago"
		,(SELECT CASE WHEN round("InsTotal") <> round("PaidToDate") THEN 'Y' ELSE 'N' END FROM PCH6 WHERE "DocEntry" = T0."DocEntry" AND UPPER(IFNULL("U_EXX_CONFTIPODET", 'No'))  = 'SI' ) AS "DetraccionPend"
		,T1."InstlmntID" AS "NroCuota"
		,0 AS "LineaAsiento"
		,T0."JrnlMemo" AS "GlosaAsiento"
		,null as "CardCodeFactoring" 
		,null as "CardNameFactoring" 
		,T0."BPLId" as "CodSucursal"
		,T0.U_EXX_PRIPAG as "CodPrioridad"
	FROM 
		OPCH T0
		LEFT JOIN PCH6 T1 ON T0."DocEntry" = T1."DocEntry"
		LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
		LEFT JOIN OCRD T3 ON T0."CardCode" = T3."CardCode"
		LEFT JOIN PCH5 T4 ON T0."DocEntry" = T4."AbsEntry" AND T4."ObjType" = T0."ObjType"
		LEFT JOIN OWHT T5 ON T4."WTCode" = T5."WTCode"
		--LEFT JOIN OCRB T4 ON T0."CardCode" = T4."CardCode"
	WHERE 
		T0."Indicator" IN ('00','01','02','08','10','14','50','99','05','91','SA')
		--T0."Indicator" IN ('01','02','03','08','DT','DO')
		AND T1."Status" = 'O'
		AND T0."U_EXC_ESCPAG"='Y'
		AND IFNULL(T1."U_EXX_CONFTIPODET",'No') = 'No'
		--AND T1."DueDate" <= :fechaVencH
		--AND (T0."DocCur" = UPPER(:monedaLoc) or T0."DocCur" = UPPER(:monedaExt))
		AND T0."CardCode" like '%' || :CardCode ||'%'
		
		--AND T0."DocEntry" NOT IN (SELECT "U_SMC_DOCENTRY" FROM "@SMC_APM_ESCDET" 
									--WHERE "U_SMC_ESCCAB" = :escenario and "U_SMC_TIPO_DOCUMENTO" = 'FT-P' AND "U_EXP_NROCUOTA"=T1."InstlmntID")
			
			/*			
		AND 
		(
		
		ifnull((select max('Y') from "@EXD_EPG1" TX0 inner join "@EXD_OEPG" TX1 on TX0."DocEntry" = TX1."DocEntry"
		where TX0."U_DOCENTRY" = T0."DocEntry" and TX0."U_NRO_CUOTA" = T1."InstlmntID" and TX0."U_TIPO_DOCUMENTO" = 'FT-P'
		
		and (ifnull((select max('Y') from "@EXP_OPMP" TT0 
		inner join "@EXP_PMP1" TT1 on TT0."DocEntry" = TT1."DocEntry"
		where TT0."Status" = 'C' and TT1."U_EXP_COD_ESCENARIOPAGO" = TX1."DocEntry" and TT1."U_EXP_TIPODOC" = '18' 
		and TT1."U_EXP_DOCENTRYDOC" = TX0."U_DOCENTRY" and TT1."U_EXP_ESTADO" = 'ER'),'N')) = 'N'
		
		and ifnull(TX1."Canceled",'') != 'Y'
		
		),'N'
		
		) = 'N' 
		
		OR T0."U_CP_VARESC"='Y')
			*/					

		--AND T0."DocTotal" NOT IN (SELECT "U_SMC_MONTO" FROM "@SMC_APM_ESCDET" WHERE "U_SMC_ESCCAB" = :escenario)
		AND IFNULL(T0."U_EXX_NUMEREND",'')=''
		
	UNION
	
	SELECT
		T0."DocEntry",
		T0."DocNum",
		T0."TaxDate" AS "FechaContable",
		T1."DueDate" AS "FechaVencimiento",
		T0."CardCode",
		T0."CardName",
		IFNULL(T0."NumAtCard",IFNULL("FolioPref",'NC01')||'-'||IFNULL("FolioNum", "DocNum")) as "NumAtCard",
		T0."DocCur",
		T0."DocRate",
		CAST( 
		(CASE 
			WHEN T0."DocCur" in ('USD','EUR') 
				THEN T1."InsTotalFC" --- T1."PaidFC" + 
				--(SELECT (R0."InsTotalFC"- R0."PaidFC") FROM PCH6 R0 WHERE R0."DocEntry" = T0."DocEntry" AND R0."InstlmntID" = '1')
			ELSE 
				T1."InsTotal" --- T1."PaidToDate" --+ 
				--(SELECT (R0."InsTotal"- R0."PaidToDate") FROM PCH6 R0 WHERE R0."DocEntry" = T0."DocEntry" AND R0."InstlmntID" = '1')
		END)AS DECIMAL(16,2)) AS "Total",
		
		IFNULL(T5."OffclCode",'') AS "CodigoRetencion" ,
		
		CAST((CASE 
			WHEN T0."DocCur" in ('USD','EUR') THEN 
				T0."WTSumFC"
			ELSE 
				T0."WTSum"
		END)AS DECIMAL(16,2)) AS "Retencion",
		CAST((CASE 	WHEN T0."DocCur" in ('USD','EUR') THEN 
							CASE (SELECT "WTCode" FROM RIN5 WHERE "AbsEntry" = T0."DocEntry")
								WHEN 'RT4C' THEN (T1."InsTotalFC" - T1."PaidFC") - ((T1."InsTotalFC"/(T1."InsTotalFC" - T1."PaidFC")) * (T4."WTAmntFC" - T4."ApplAmntFC"))
								WHEN 'RIGV' THEN (T1."InsTotalFC" - T1."PaidFC") - ((T1."InsTotalFC"/(T1."InsTotalFC" - T1."PaidFC")) * (T4."WTAmntFC" - T4."ApplAmntFC"))
								ELSE T1."InsTotalFC" - T1."PaidFC"--+ T0."WTSumFC" - T4."ApplAmntFC"
							END
						ELSE 
							CASE (SELECT "WTCode" FROM RIN5 WHERE "AbsEntry" = T0."DocEntry")
								WHEN 'RT4C' THEN (T1."InsTotal" - T1."PaidToDate") - ((T1."InsTotal" /(T1."InsTotal" - T0."PaidToDate")) * (T4."WTAmnt" - T4."ApplAmnt")) 
								WHEN 'RIGV' THEN (T1."InsTotal" - T1."PaidToDate") - ((T1."InsTotal" /(T1."InsTotal" - T0."PaidToDate")) * (T4."WTAmnt" - T4."ApplAmnt")) 								
								ELSE T1."InsTotal" - T1."PaidToDate"-- + T0."WTSum" - T4."ApplAmnt"
							END
			END)AS DECIMAL(16,2)) AS "TotalPagar",
		T3."LicTradNum" AS "RUC",
		
		
		
		/*
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."Account") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" --*AND "BankCode" = :tipoBanco
			AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,'')*/
		(SELECT CASE T0."U_EXC_PAGBEN"
			WHEN 'Y' THEN
			CASE  R2."BankCode" WHEN R2."BankCode" THEN R2."Account" ELSE R2."U_EXM_INTERBANCARIA" END
			ELSE CASE :tipoBanco WHEN '000' THEN R3."Account" ELSE CASE WHEN :tipoBanco = R3."BankCode" THEN R3."Account" ELSE R3."U_EXM_INTERBANCARIA" END END END
		FROM OCRD R0
		LEFT JOIN OCPR R1 ON R0."CardCode"=R1."CardCode" AND R1."U_EXC_BENEFI"='Y'
		LEFT JOIN OCRB R2 ON R0."CardCode"=R2."CardCode" AND R1."Name"=R2."U_EXC_BENEFI" AND IFNULL(R2."UsrNumber1",'') = T0."DocCur" --beneficiario
		LEFT JOIN OCRB R3 ON R0."CardCode"=R3."CardCode" AND R3."U_EXC_ACTIVO"='Y' AND IFNULL(R3."UsrNumber1",'') = T0."DocCur" --normal
		WHERE R0."CardCode" = T0."CardCode")
		AS "Cuenta",
		
		/*
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" --AND "BankCode" = :tipoBanco
			AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,'') */
		(SELECT CASE T0."U_EXC_PAGBEN" WHEN 'Y' THEN R2."UsrNumber1" ELSE R3."UsrNumber1" END
		FROM OCRD R0
		LEFT JOIN OCPR R1 ON R0."CardCode"=R1."CardCode" AND R1."U_EXC_BENEFI"='Y'
		LEFT JOIN OCRB R2 ON R0."CardCode"=R2."CardCode" AND R1."Name"=R2."U_EXC_BENEFI" AND IFNULL(R2."UsrNumber1",'') = T0."DocCur" --beneficiario
		LEFT JOIN OCRB R3 ON R0."CardCode"=R3."CardCode" AND R3."U_EXC_ACTIVO"='Y' AND IFNULL(R3."UsrNumber1",'') = T0."DocCur" --normal
		WHERE R0."CardCode" = T0."CardCode")
		AS "CuentaMoneda",
		
		/*
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" --AND "BankCode" = :tipoBanco
			AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,'')*/
		(SELECT CASE T0."U_EXC_PAGBEN" WHEN 'Y' THEN R2."BankCode" ELSE R3."BankCode" END
		FROM OCRD R0
		LEFT JOIN OCPR R1 ON R0."CardCode"=R1."CardCode" AND R1."U_EXC_BENEFI"='Y'
		LEFT JOIN OCRB R2 ON R0."CardCode"=R2."CardCode" AND R1."Name"=R2."U_EXC_BENEFI" AND IFNULL(R2."UsrNumber1",'') = T0."DocCur"--beneficiario
		LEFT JOIN OCRB R3 ON R0."CardCode"=R3."CardCode" AND R3."U_EXC_ACTIVO"='Y' AND IFNULL(R3."UsrNumber1",'') = T0."DocCur" --normal
		WHERE R0."CardCode" = T0."CardCode")
		AS "BankCode",
		(CASE 
			WHEN DAYS_BETWEEN(T0."DocDueDate", :fechaVencH) < 0 THEN 0 
			ELSE DAYS_BETWEEN(T0."DocDueDate", :fechaVencH) 
		END) AS "Atraso",
		--0 AS "Atraso",
		'' AS "Marca",
		'' AS "Estado",
		'NC-C' as "Documento",
		T0."Comments" AS "Comentario",
		CASE WHEN T3."QryGroup11" = 'Y' THEN 'PROV. CAJA CHICA' ELSE '' END AS "Propiedad",
		T1."DueDate"
		,IFNULL(T0."U_EXC_ORIGEN",'') as "Origen"
		,IFNULL(T0."PayBlock", 'N') AS "BloqueoPago"
		,'N' AS "DetraccionPend"
	    ,T1."InstlmntID" AS "NroCuota"
		,0 AS "LineaAsiento"
		,T0."JrnlMemo" AS "GlosaAsiento"
		,null as "CardCodeFactoring" 
		,null as "CardNameFactoring" 
		,T0."BPLId" as "CodSucursal"
		,T0.U_EXX_PRIPAG as "CodPrioridad"
	FROM 
		ORIN T0
		LEFT JOIN RIN6 T1 ON T0."DocEntry" = T1."DocEntry"
		LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
		LEFT JOIN OCRD T3 ON T0."CardCode" = T3."CardCode"
		LEFT JOIN PCH5 T4 ON T0."DocEntry" = T4."AbsEntry" AND T4."ObjType" = T0."ObjType"
		LEFT JOIN OWHT T5 ON T4."WTCode" = T5."WTCode"
		--LEFT JOIN OCRB T4 ON T0."CardCode" = T4."CardCode"
	WHERE 
		T0."Indicator" IN ('00','01','02','08','14','50','99','91','05','07','SA')
		--T0."Indicator" IN ('01','02','03','08','DT','DO')
		AND T1."InstlmntID" = '1'
		AND T1."Status" = 'O'
		AND T0."U_EXC_ESCPAG"='Y'
		AND T2."PymntGroup" not like '%DT%'
		--AND T1."DueDate" <= :fechaVencH
		--AND (T0."DocCur" = UPPER(:monedaLoc) or T0."DocCur" = UPPER(:monedaExt))
		AND T0."CardCode" like '%' || :CardCode ||'%'
		--AND T0."DocEntry" NOT IN (SELECT "U_SMC_DOCENTRY" FROM "@SMC_APM_ESCDET" 
		--							WHERE "U_SMC_ESCCAB" = :escenario and "U_SMC_TIPO_DOCUMENTO" = 'NC-C')
		AND (ifnull((select max('Y') from "@EXD_EPG1" TX0 inner join "@EXD_OEPG" TX1 on TX0."DocEntry" = TX1."DocEntry"
		where TX0."U_DOCENTRY" = T0."DocEntry" and TX0."U_TIPO_DOCUMENTO" = 'NC-C' and ifnull(TX1."Canceled",'') != 'Y'),'N') = 'N' OR T0."U_CP_VARESC"='Y')
		--AND T0."DocTotal" NOT IN (SELECT "U_SMC_MONTO" FROM "@SMC_APM_ESCDET" WHERE "U_SMC_ESCCAB" = :escenario)
		
		
		UNION	
		
		SELECT
		T0."DocEntry",
		T0."DocNum",
		T0."TaxDate" AS "FechaContable",
		T1."DueDate" AS "FechaVencimiento",
		T0."CardCode",
		T0."CardName",
		T0."NumAtCard",
		T0."DocCur",
		T0."DocRate",
		CAST( 
		(CASE 
			WHEN T0."DocCur" in ('USD','EUR') 
				THEN T1."InsTotalFC" --- T1."PaidFC" + 
				--(SELECT (R0."InsTotalFC"- R0."PaidFC") FROM PCH6 R0 WHERE R0."DocEntry" = T0."DocEntry" AND R0."InstlmntID" = '1')
			ELSE 
				T1."InsTotal" --- T1."PaidToDate" --+ 
				--(SELECT (R0."InsTotal"- R0."PaidToDate") FROM PCH6 R0 WHERE R0."DocEntry" = T0."DocEntry" AND R0."InstlmntID" = '1')
		END)AS DECIMAL(16,2)) AS "Total",
		
		IFNULL(T5."OffclCode",'') AS "CodigoRetencion" ,
				
		CAST((CASE 
			WHEN T0."DocCur" in ('USD','EUR') THEN 
				T0."WTSumFC"
			ELSE 
				T0."WTSum"
		END)AS DECIMAL(16,2)) AS "Retencion",
		CAST((CASE 	WHEN T0."DocCur" in ('USD','EUR') THEN 
							CASE (SELECT "WTCode" FROM DPO5 WHERE "AbsEntry" = T0."DocEntry")
								WHEN 'RT4C' THEN T1."InsTotalFC" - T1."PaidFC"
								ELSE T1."InsTotalFC" - T1."PaidFC"- T0."WTSumFC"
							END
						ELSE 
							CASE (SELECT "WTCode" FROM DPO5 WHERE "AbsEntry" = T0."DocEntry")
								WHEN 'RT4C' THEN T1."InsTotal" - T1."PaidToDate"
								ELSE T1."InsTotal" - T1."PaidToDate" - T0."WTSum"
							END
			END)AS DECIMAL(16,2)) AS "TotalPagar",
		T3."LicTradNum" AS "RUC",
		
		
		
		/*
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."Account") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" --*AND "BankCode" = :tipoBanco
			AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,'')*/
		(SELECT CASE T0."U_EXC_PAGBEN"
			WHEN 'Y' THEN
				CASE :tipoBanco WHEN R2."BankCode" THEN R2."Account" ELSE R2."U_EXM_INTERBANCARIA" END
			ELSE CASE :tipoBanco WHEN R3."BankCode" THEN R3."Account" ELSE R3."U_EXM_INTERBANCARIA" END END
		FROM OCRD R0
		LEFT JOIN OCPR R1 ON R0."CardCode"=R1."CardCode" AND R1."U_EXC_BENEFI"='Y'
		LEFT JOIN OCRB R2 ON R0."CardCode"=R2."CardCode" AND R1."Name"=R2."U_EXC_BENEFI" AND IFNULL(R2."UsrNumber1",'') = T0."DocCur" --:monedaLoc --beneficiario
		LEFT JOIN OCRB R3 ON R0."CardCode"=R3."CardCode" AND R3."U_EXC_ACTIVO"='Y' AND IFNULL(R3."UsrNumber1",'') = T0."DocCur" --:monedaLoc --normal
		WHERE R0."CardCode" = T0."CardCode")
		AS "Cuenta",
		
		/*
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" --AND "BankCode" = :tipoBanco
			AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,'') */
		(SELECT CASE T0."U_EXC_PAGBEN" WHEN 'Y' THEN R2."UsrNumber1" ELSE R3."UsrNumber1" END
		FROM OCRD R0
		LEFT JOIN OCPR R1 ON R0."CardCode"=R1."CardCode" AND R1."U_EXC_BENEFI"='Y'
		LEFT JOIN OCRB R2 ON R0."CardCode"=R2."CardCode" AND R1."Name"=R2."U_EXC_BENEFI" AND IFNULL(R2."UsrNumber1",'') = T0."DocCur" --:monedaLoc --beneficiario
		LEFT JOIN OCRB R3 ON R0."CardCode"=R3."CardCode" AND R3."U_EXC_ACTIVO"='Y' AND IFNULL(R3."UsrNumber1",'') = T0."DocCur" --:monedaLoc --normal
		WHERE R0."CardCode" = T0."CardCode")
		AS "CuentaMoneda",
		
		/*
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" --AND "BankCode" = :tipoBanco
			AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,'')*/
		(SELECT CASE T0."U_EXC_PAGBEN" WHEN 'Y' THEN R2."BankCode" ELSE R3."BankCode" END
		FROM OCRD R0
		LEFT JOIN OCPR R1 ON R0."CardCode"=R1."CardCode" AND R1."U_EXC_BENEFI"='Y'
		LEFT JOIN OCRB R2 ON R0."CardCode"=R2."CardCode" AND R1."Name"=R2."U_EXC_BENEFI" AND IFNULL(R2."UsrNumber1",'') = T0."DocCur" --:monedaLoc --beneficiario
		LEFT JOIN OCRB R3 ON R0."CardCode"=R3."CardCode" AND R3."U_EXC_ACTIVO"='Y' AND IFNULL(R3."UsrNumber1",'') = T0."DocCur" --:monedaLoc --normal
		WHERE R0."CardCode" = T0."CardCode")
		AS "BankCode",
		(CASE 
			WHEN DAYS_BETWEEN(T0."DocDueDate", :fechaVencH) < 0 THEN 0 
			ELSE DAYS_BETWEEN(T0."DocDueDate", :fechaVencH) 
		END) AS "Atraso",
		--0 AS "Atraso",
		'' AS "Marca",
		'' AS "Estado",
		'FA-P' as "Documento",
		T0."Comments" AS "Comentario",
		CASE WHEN T3."QryGroup11" = 'Y' THEN 'PROV. CAJA CHICA' ELSE '' END AS "Propiedad",
		T1."DueDate"
		,IFNULL(T0."U_EXC_ORIGEN",'') as "Origen"
		,IFNULL(T0."PayBlock", 'N') AS "BloqueoPago"
		,IFNULL((select MAX('Y') from JDT1 TX0 where TX0."ShortName" = T0."CardCode" and TX0."Ref2" = T0."NumAtCard" and U_EXX_DETRACCION = 'Y'
		and case when ifnull(TX0."FCCurrency",'') <> 'SOL' then TX0."BalDueCred" else TX0."BalFcCred" end > 0),'N')  AS "DetraccionPend"
		,T1."InstlmntID" AS "NroCuota"
		,0 AS "LineaAsiento"
		,T0."JrnlMemo" AS "GlosaAsiento"
		,null as "CardCodeFactoring" 
		,null as "CardNameFactoring" 
		,T0."BPLId" as "CodSucursal"
		,T0.U_EXX_PRIPAG as "CodPrioridad"
	FROM 
		ODPO T0
		LEFT JOIN DPO6 T1 ON T0."DocEntry" = T1."DocEntry"
		LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
		LEFT JOIN OCRD T3 ON T0."CardCode" = T3."CardCode"
		LEFT JOIN PCH5 T4 ON T0."DocEntry" = T4."AbsEntry" AND T4."ObjType" = T0."ObjType"
		LEFT JOIN OWHT T5 ON T4."WTCode" = T5."WTCode"
		--LEFT JOIN OCRB T4 ON T0."CardCode" = T4."CardCode"
	WHERE 
		T0."Indicator" IN ('SA','00','01','02','08','14','50','99','91','05','07')
		--T0."Indicator" IN ('01','02','03','08','DT','DO')
		AND T1."InstlmntID" = '1'
		AND T1."Status" = 'O'
		AND T0."U_EXC_ESCPAG"='Y'
		AND T2."PymntGroup" not like '%DT%'
		--AND T1."DueDate" <= :fechaVencH
		--AND (T0."DocCur" = UPPER(:monedaLoc) or T0."DocCur" = UPPER(:monedaExt))
		AND T0."CardCode" like '%' || :CardCode ||'%'
		AND T0."CreateTran" = 'Y'
		--AND T0."DocEntry" NOT IN (SELECT "U_SMC_DOCENTRY" FROM "@SMC_APM_ESCDET" 
									--WHERE "U_SMC_ESCCAB" = :escenario and "U_SMC_TIPO_DOCUMENTO" = 'FA-P')
		AND (ifnull((select max('Y') from "@EXD_EPG1" TX0 inner join "@EXD_OEPG" TX1 on TX0."DocEntry" = TX1."DocEntry"
		where TX0."U_DOCENTRY" = T0."DocEntry" and TX0."U_TIPO_DOCUMENTO" = 'FA-P' and ifnull(TX1."Canceled",'') !='Y'),'N') = 'N' OR T0."U_CP_VARESC"='Y')
		--AND T0."DocTotal" NOT IN (SELECT "U_SMC_MONTO" FROM "@SMC_APM_ESCDET" WHERE "U_SMC_ESCCAB" = :escenario)
		
		
		union
		
		
		SELECT
		T0."DocEntry",
		T0."DocNum",
		T0."TaxDate" AS "FechaContable",
		T1."DueDate" AS "FechaVencimiento",
		T0."CardCode",
		T0."CardName",
		T0."NumAtCard",
		T0."DocCur",
		T0."DocRate",
		CAST( 
		(CASE 
			WHEN T0."DocCur" in ('USD','EUR') 
				THEN T1."InsTotalFC" --- T1."PaidFC" + 
				--(SELECT (R0."InsTotalFC"- R0."PaidFC") FROM PCH6 R0 WHERE R0."DocEntry" = T0."DocEntry" AND R0."InstlmntID" = '1')
			ELSE 
				T1."InsTotal" --- T1."PaidToDate" --+ 
				--(SELECT (R0."InsTotal"- R0."PaidToDate") FROM PCH6 R0 WHERE R0."DocEntry" = T0."DocEntry" AND R0."InstlmntID" = '1')
		END)AS DECIMAL(16,2)) AS "Total",
		
		IFNULL(T5."OffclCode",'') AS "CodigoRetencion" ,
				
		CAST((CASE 
			WHEN T0."DocCur" in ('USD','EUR') THEN 
				T0."WTSumFC"
			ELSE 
				T0."WTSum"
		END)AS DECIMAL(16,2)) AS "Retencion",
		CAST((CASE 	WHEN T0."DocCur" in ('USD','EUR') THEN 
							CASE (SELECT "WTCode" FROM DPO5 WHERE "AbsEntry" = T0."DocEntry")
								WHEN 'RT4C' THEN T1."InsTotalFC" - ((T1."InsTotalFC"/(T1."InsTotalFC" - T1."PaidFC")) * (T4."WTAmntFC" - T4."ApplAmntFC"))
								WHEN 'RIGV' THEN T1."InsTotalFC" - ((T1."InsTotalFC"/(T1."InsTotalFC" - T1."PaidFC")) * (T4."WTAmntFC" - T4."ApplAmntFC"))
								ELSE T1."InsTotalFC" - T1."PaidFC"- T0."WTSumFC"
							END
						ELSE 
							CASE (SELECT "WTCode" FROM DPO5 WHERE "AbsEntry" = T0."DocEntry")
								WHEN 'RT4C' THEN T1."InsTotal" - T1."PaidToDate"
								ELSE T1."InsTotal" - T1."PaidToDate" - T0."WTSum"
							END
			END)AS DECIMAL(16,2)) AS "TotalPagar",
		T3."LicTradNum" AS "RUC",
		
		
		
		/*
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."Account") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" --*AND "BankCode" = :tipoBanco
			AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."U_EXM_INTERBANCARIA") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,'')*/
		(SELECT CASE T0."U_EXC_PAGBEN"
			WHEN 'Y' THEN
				CASE R2."BankCode" WHEN R2."BankCode" THEN R2."Account" ELSE R2."U_EXM_INTERBANCARIA" END
				ELSE CASE :tipoBanco WHEN '000' THEN R3."Account" ELSE CASE WHEN :tipoBanco = R3."BankCode" THEN R3."Account" ELSE R3."U_EXM_INTERBANCARIA" END END END
		FROM OCRD R0
		LEFT JOIN OCPR R1 ON R0."CardCode"=R1."CardCode" AND R1."U_EXC_BENEFI"='Y'
		LEFT JOIN OCRB R2 ON R0."CardCode"=R2."CardCode" AND R1."Name"=R2."U_EXC_BENEFI" AND IFNULL(R2."UsrNumber1",'') = T0."DocCur"--:monedaLoc --beneficiario
		LEFT JOIN OCRB R3 ON R0."CardCode"=R3."CardCode" AND R3."U_EXC_ACTIVO"='Y' AND IFNULL(R3."UsrNumber1",'') = T0."DocCur"--:monedaLoc --normal
		WHERE R0."CardCode" = T0."CardCode")
		AS "Cuenta",
		
		/*
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" --AND "BankCode" = :tipoBanco
			AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."UsrNumber1") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,'') */
		(SELECT CASE T0."U_EXC_PAGBEN" WHEN 'Y' THEN R2."UsrNumber1" ELSE R3."UsrNumber1" END
		FROM OCRD R0
		LEFT JOIN OCPR R1 ON R0."CardCode"=R1."CardCode" AND R1."U_EXC_BENEFI"='Y'
		LEFT JOIN OCRB R2 ON R0."CardCode"=R2."CardCode" AND R1."Name"=R2."U_EXC_BENEFI" AND IFNULL(R2."UsrNumber1",'') = T0."DocCur"--:monedaLoc --beneficiario
		LEFT JOIN OCRB R3 ON R0."CardCode"=R3."CardCode" AND R3."U_EXC_ACTIVO"='Y' AND IFNULL(R3."UsrNumber1",'') = T0."DocCur"--:monedaLoc --normal
		WHERE R0."CardCode" = T0."CardCode")
		AS "CuentaMoneda",
		
		/*
		IFNULL(IFNULL(IFNULL(IFNULL(IFNULL(
		IFNULL(
		(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" --AND "BankCode" = :tipoBanco
			AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y')
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '011' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '002' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y')) 
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '009' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '022' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,(SELECT MAX(R0."BankCode") FROM OCRB R0 WHERE R0."CardCode" = T0."CardCode" AND "BankCode" = '003' AND (IFNULL(R0."UsrNumber1",:monedaLoc) = :monedaLoc)and "U_EXC_ACTIVO" = 'Y'))
		,'')*/
		(SELECT CASE T0."U_EXC_PAGBEN" WHEN 'Y' THEN R2."BankCode" ELSE R3."BankCode" END
		FROM OCRD R0
		LEFT JOIN OCPR R1 ON R0."CardCode"=R1."CardCode" AND R1."U_EXC_BENEFI"='Y'
		LEFT JOIN OCRB R2 ON R0."CardCode"=R2."CardCode" AND R1."Name"=R2."U_EXC_BENEFI" AND IFNULL(R2."UsrNumber1",'') = T0."DocCur"--:monedaLoc --beneficiario
		LEFT JOIN OCRB R3 ON R0."CardCode"=R3."CardCode" AND R3."U_EXC_ACTIVO"='Y' AND IFNULL(R3."UsrNumber1",'') = T0."DocCur"--:monedaLoc --normal
		WHERE R0."CardCode" = T0."CardCode")
		AS "BankCode",
		
		(CASE 
			WHEN DAYS_BETWEEN(T0."DocDueDate", :fechaVencH) < 0 THEN 0 
			ELSE DAYS_BETWEEN(T0."DocDueDate", :fechaVencH) 
		END) AS "Atraso",
		--0 AS "Atraso",
		'' AS "Marca",
		'' AS "Estado",
		'SA-P' as "Documento",
		T0."Comments" AS "Comentario",
		CASE WHEN T3."QryGroup11" = 'Y' THEN 'PROV. CAJA CHICA' ELSE '' END AS "Propiedad",
		T1."DueDate"
		,IFNULL(T0."U_EXC_ORIGEN",'') as "Origen"
		,IFNULL(T0."PayBlock", 'N') AS "BloqueoPago"
		,'N' AS "DetraccionPend"
		,T1."InstlmntID" AS "NroCuota"
		,0 AS "LineaAsiento"
		,T0."JrnlMemo" AS "GlosaAsiento"
		,null as "CardCodeFactoring" 
		,null as "CardNameFactoring"
		,T0."BPLId" as "CodSucursal"
		,T0.U_EXX_PRIPAG as "CodPrioridad"
	FROM 
		ODPO T0
		LEFT JOIN DPO6 T1 ON T0."DocEntry" = T1."DocEntry"
		LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
		LEFT JOIN OCRD T3 ON T0."CardCode" = T3."CardCode"
		LEFT JOIN PCH5 T4 ON T0."DocEntry" = T4."AbsEntry" AND T4."ObjType" = T0."ObjType"
		LEFT JOIN OWHT T5 ON T4."WTCode" = T5."WTCode"
		--LEFT JOIN OCRB T4 ON T0."CardCode" = T4."CardCode"
	WHERE 
		T0."Indicator" IN ('00','01','02','08','14','50','99','91','05','07','SA')
		--T0."Indicator" IN ('01','02','03','08','DT','DO')
		AND T1."InstlmntID" = '1'
		AND T1."Status" = 'O'
		AND T0."U_EXC_ESCPAG"='Y'
		AND T2."PymntGroup" not like '%DT%'
		--AND T1."DueDate" <= :fechaVencH
		--AND (T0."DocCur" = UPPER(:monedaLoc) or T0."DocCur" = UPPER(:monedaExt))
		AND T0."CardCode" like '%' || :CardCode ||'%'
		AND T0."CreateTran" = 'N'
		--AND T0."DocEntry" NOT IN (SELECT "U_SMC_DOCENTRY" FROM "@SMC_APM_ESCDET" 
		--							WHERE "U_SMC_ESCCAB" = :escenario and "U_SMC_TIPO_DOCUMENTO" = 'SA-P')
		AND (ifnull((select max('Y') from "@EXD_EPG1" TX0 inner join "@EXD_OEPG" TX1 on TX0."DocEntry" = TX1."DocEntry"
		where TX0."U_DOCENTRY" = T0."DocEntry" and TX0."U_TIPO_DOCUMENTO" = 'SA-P' and ifnull(TX1."Canceled",'') != 'Y'),'N') = 'N'OR T0."U_CP_VARESC"='Y')
		--AND T0."DocTotal" NOT IN (SELECT "U_SMC_MONTO" FROM "@SMC_APM_ESCDET" WHERE "U_SMC_ESCCAB" = :escenario)
		
		
	union all	
	
	SELECT
		T0."DocEntry",
		T0."DocNum",
		T0."TaxDate" AS "FechaContable",
		T0."DocDueDate" AS "FechaVencimiento",
		T0."CardCode",
		T0."CardName",
		T0."U_EXX_NUMEREND" as "NumAtCard",
		T0."DocCurr",
		T0."DocRate",
		CAST( 
		(CASE 
			WHEN T0."DocCurr" in ('USD','EUR') 
				THEN T0."NoDocSumFC" 
			ELSE 
				T0."NoDocSum"
		END)AS DECIMAL(16,2)) AS "Total",
		
		'' AS "CodigoRetencion" ,
				
		0 AS "Retencion",
		CAST( 
		(CASE 
			WHEN T0."DocCurr" in ('USD','EUR') 
				THEN T0."NoDocSumFC" 
			ELSE 
				T0."NoDocSum"
		END)AS DECIMAL(16,2)) AS "TotalPagar",
			
		(select "LicTradNum" from OCRD where "CardCode" = T0."CardCode") AS "RUC",

		(SELECT CASE :tipoBanco WHEN '000' THEN R3."Account" ELSE CASE WHEN :tipoBanco = R3."BankCode" THEN R3."Account" ELSE R3."U_EXM_INTERBANCARIA" END END 
		FROM OCRD R0
		LEFT JOIN OCRB R3 ON R0."CardCode"=R3."CardCode" AND R3."U_EXC_ACTIVO"='Y' AND IFNULL(R3."UsrNumber1",'') = T0."DocCurr"--:monedaLoc --normal
		WHERE R0."CardCode" = T3."CardCode")
		AS "Cuenta",
		
		(SELECT R3."UsrNumber1"
		FROM OCRD R0
		LEFT JOIN OCRB R3 ON R0."CardCode"=R3."CardCode" AND R3."U_EXC_ACTIVO"='Y' AND IFNULL(R3."UsrNumber1",'') = T0."DocCurr"--:monedaLoc --normal
		WHERE R0."CardCode" = T3."CardCode")
		AS "CuentaMoneda",

		(SELECT R3."BankCode"
		FROM OCRD R0
		LEFT JOIN OCRB R3 ON R0."CardCode"=R3."CardCode" AND R3."U_EXC_ACTIVO"='Y' AND IFNULL(R3."UsrNumber1",'') = T0."DocCurr"--normal
		WHERE R0."CardCode" = T3."CardCode")
		AS "BankCode",
		(CASE 
			WHEN DAYS_BETWEEN(T0."DocDueDate", :fechaVencH) < 0 THEN 0 
			ELSE DAYS_BETWEEN(T0."DocDueDate", :fechaVencH) 
		END) AS "Atraso",
		'' AS "Marca",
		'' AS "Estado",
		'SP' as "Documento",
		T0."Comments" AS "Comentario",
		CASE WHEN T3."QryGroup11" = 'Y' THEN 'PROV. CAJA CHICA' ELSE '' END AS "Propiedad",
		T0."DocDueDate"
		,'' as "Origen"
		,'N' AS "BloqueoPago"
		,'N' AS "DetraccionPend"
		,1 AS "NroCuota"
		,0 AS "LineaAsiento"
		,T0."JrnlMemo" AS "GlosaAsiento"
		,null as "CardCodeFactoring" 
		,null as "CardNameFactoring" 
		,T0."BPLId" as "CodSucursal"
		,T0.U_EXX_PRIPAG as "CodPrioridad"
	FROM 
		OPDF T0
		LEFT JOIN OCRD T3 ON T0."CardCode" = T3."CardCode"
	WHERE 
		--AND T1."InstlmntID" = '1'
		1=1
		--T0."DocDueDate" <= :fechaVencH
		AND T0."Canceled" = 'N'
		--AND (T0."DocCurr" = UPPER(:monedaLoc) or T0."DocCurr" = UPPER(:monedaExt))
		AND T0."CardCode" like '%' || :CardCode ||'%'
		--AND T0."DocEntry" NOT IN (SELECT "U_SMC_DOCENTRY" FROM "@SMC_APM_ESCDET" 
		--							WHERE "U_SMC_ESCCAB" = :escenario and "U_SMC_TIPO_DOCUMENTO" = 'SP')
		AND 
		(
		
		ifnull((select max('Y') from "@EXD_EPG1" TX0 inner join "@EXD_OEPG" TX1 on TX0."DocEntry" = TX1."DocEntry"
		where TX0."U_DOCENTRY" = T0."DocEntry" and TX0."U_TIPO_DOCUMENTO" = 'SP' 
		
		and (ifnull((select max('Y') from "@EXP_OPMP" TT0 
		inner join "@EXP_PMP1" TT1 on TT0."DocEntry" = TT1."DocEntry"
		where TT0."Status" = 'C' and TT1."U_EXP_COD_ESCENARIOPAGO" = TX1."DocEntry" and TT1."U_EXP_TIPODOC" = '140' 
		and TT1."U_EXP_DOCENTRYDOC" = TX0."U_DOCENTRY" and TT1."U_EXP_ESTADO" = 'ER'),'N')) = 'N'
		
		and ifnull(TX1."Canceled",'') != 'Y'
		
		),'N'
		
		) = 'N' 
		
		OR T0."U_CP_VARESC"='Y')
	
		UNION	
	--------pagos recibicos-------

		SELECT
		DISTINCT
		T0."TransId" as "DocEntry",
		T0."Number" as "DocNum",
		T0."TaxDate" AS "FechaContable",
		T1."DueDate" AS "FechaVencimiento",
		T3."CardCode",
		T3."CardName",
		(select max(TX0."U_EXX_NUMEREND") from ORCT TX0 where TX0."TransId" = T0."TransId") as "NumAtCard",
		--T0."TransCurr"
		IFNULL(T1."FCCurrency",'SOL'),
		case when ifnull(T1."FCCurrency",'') = '' then 1.00 else T1."FCCredit"/case when T1."Credit" = 0 then 1 else T1."Credit" end end,
		CAST( 
		(CASE 
			WHEN IFNULL(T1."FCCurrency",'SOL') in ('USD','EUR') 
			
				THEN (T1."FCCredit" - IFNULL((select sum(TT0."ReconSumFC") from 
		"ITR1" TT0 where TT0."TransId" = T0."TransId" AND TT0."TransRowId"= T1."Line_ID"
		GROUP BY TT0."TransId",TT0."TransRowId"),2)) 
		
			ELSE 
			
				(T1."Credit" - IFNULL((select sum(TT0."ReconSum") from 
		"ITR1" TT0 where TT0."TransId" = T0."TransId" AND TT0."TransRowId"= T1."Line_ID"
		GROUP BY TT0."TransId",TT0."TransRowId"),0))
		
		END)AS DECIMAL(16,2)) AS "Total",
		
		'' AS "CodigoRetencion" ,
		0 AS "Retencion",
		CAST( 
		(CASE 
			WHEN IFNULL(T1."FCCurrency",'SOL') in ('USD','EUR') 
			
				THEN (T1."FCCredit" - IFNULL((select sum(TT0."ReconSumFC") from 
		"ITR1" TT0 where TT0."TransId" = T0."TransId" AND TT0."TransRowId"= T1."Line_ID"
		GROUP BY TT0."TransId",TT0."TransRowId"),0)) 
		
			ELSE 
			
				(T1."Credit" - IFNULL((select sum(TT0."ReconSum") from 
		"ITR1" TT0 where TT0."TransId" = T0."TransId" AND TT0."TransRowId"= T1."Line_ID"
		GROUP BY TT0."TransId",TT0."TransRowId"),0))
		
		END)AS DECIMAL(16,2)) AS "TotalPagar",
			
		T3."LicTradNum" AS "RUC",

		(SELECT CASE :tipoBanco WHEN '000' THEN R3."Account" ELSE CASE WHEN :tipoBanco = R3."BankCode" THEN R3."Account" ELSE R3."U_EXM_INTERBANCARIA" END END 
		FROM OCRD R0
		LEFT JOIN OCRB R3 ON R0."CardCode"=R3."CardCode" AND R3."U_EXC_ACTIVO"='Y' AND IFNULL(R3."UsrNumber1",'') = IFNULL(T1."FCCurrency",'SOL')--:monedaLoc --normal
		WHERE R0."CardCode" = T3."CardCode")
		AS "Cuenta",
		
		(SELECT R3."UsrNumber1"
		FROM OCRD R0
		LEFT JOIN OCRB R3 ON R0."CardCode"=R3."CardCode" AND R3."U_EXC_ACTIVO"='Y' AND IFNULL(R3."UsrNumber1",'') = IFNULL(T1."FCCurrency",'SOL')--:monedaLoc --normal
		WHERE R0."CardCode" = T3."CardCode")
		AS "CuentaMoneda",

		(SELECT R3."BankCode"
		FROM OCRD R0
		LEFT JOIN OCRB R3 ON R0."CardCode"=R3."CardCode" AND R3."U_EXC_ACTIVO"='Y' AND IFNULL(R3."UsrNumber1",'') = IFNULL(T1."FCCurrency",'SOL')--:monedaLoc --normal
		WHERE R0."CardCode" = T3."CardCode")
		AS "BankCode",
		(CASE 
			WHEN DAYS_BETWEEN(T0."DueDate", :fechaVencH) < 0 THEN 0 
			ELSE DAYS_BETWEEN(T0."DueDate", :fechaVencH) 
		END) AS "Atraso",
		--0 AS "Atraso",
		'' AS "Marca",
		'' AS "Estado",
		'PR' as "Documento",
		'' AS "Comentario",
		CASE WHEN T3."QryGroup11" = 'Y' THEN 'PROV. CAJA CHICA' ELSE '' END AS "Propiedad",
		T0."DueDate"
		,'' as "Origen"
		,T1."PayBlock" AS "BloqueoPago"
		,'N' AS "DetraccionPend"
		,0 AS "NroCuota"
		,T1."Line_ID" AS "LineaAsiento"
		,T0."Memo" AS "GlosaAsiento"
		,T0."U_EXX_PROORI" as "CardCodeFactoring" 
		,T4."CardName" as "CardNameFactoring" 
		,T1."BPLId" as "CodSucursal"
		,T0.U_EXX_PRIPAG as "CodPrioridad"
	FROM 
		OJDT T0
		LEFT JOIN JDT1 T1 ON T0."TransId" = T1."TransId"
		--LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
		inner JOIN OCRD T3 ON T1."ShortName" = T3."CardCode"
		LEFT JOIN OCRD T4 ON T0."U_EXX_PROORI" = T4."CardCode"
		--LEFT JOIN OCRB T4 ON T0."CardCode" = T4."CardCode"
	WHERE 
	 	T0."TransType" = 24

		/*
		AND IFNULL((select count(*) from "ITR1" TT0 where TT0."TransId" = T0."TransId" GROUP BY TT0."TransId"),0) = 0
		*/
		and (T1."Credit" - 
		IFNULL((select sum(TT0."ReconSum") from 
		"ITR1" TT0 where TT0."TransId" = T0."TransId" AND TT0."TransRowId"= T1."Line_ID"
		GROUP BY TT0."TransId",TT0."TransRowId"),0)) > 0
		

		--AND T1."DueDate" <= :fechaVencH
		--AND (IFNULL(T1."FCCurrency",'SOL') = UPPER(:monedaLoc) or IFNULL(T1."FCCurrency",'SOL') = UPPER(:monedaExt))
		AND T3."CardCode" like '%' || :CardCode ||'%'
		and T1."Credit">0
		--AND T0."TransId" NOT IN (SELECT "U_SMC_DOCENTRY" FROM "@SMC_APM_ESCDET" 
		--							WHERE "U_SMC_ESCCAB" = :escenario and "U_SMC_TIPO_DOCUMENTO" = 'PR')
		AND (ifnull((select max('Y') from "@EXD_EPG1" TX0 inner join "@EXD_OEPG" TX1 on TX0."DocEntry" = TX1."DocEntry"
		where TX0."U_DOCENTRY" = T0."TransId" and TX0."U_TIPO_DOCUMENTO" = 'PR' and ifnull(TX1."Canceled",'') != 'Y'),'N') = 'N' OR T0."U_CP_VARESC"='Y')
		
		-------pagos recibidos-------
		union all
		---------AS-------------------------
		SELECT
		DISTINCT
		T0."TransId" as "DocEntry",
		T0."Number" as "DocNum",
		T0."TaxDate" AS "FechaContable",
		T1."DueDate" AS "FechaVencimiento",
		T3."CardCode",
		T3."CardName",
		T0."Ref2" 	as "NumAtCard",
		--T0."TransCurr"
		ifnull(T1."FCCurrency",'SOL'),
		case when ifnull(T1."FCCurrency",'') = '' then 1.00 else T1."FCCredit"/case when T1."Credit" = 0 then 1 else T1."Credit" end end,
		CAST( 
		(CASE 
			WHEN ifnull(T1."FCCurrency",'SOL') in ('USD','EUR') 
			
				THEN (T1."FCCredit" - ifnull((select sum(TT0."ReconSumFC") from 
		"ITR1" TT0 where TT0."TransId" = T0."TransId"  AND TT0."TransRowId" = T1."Line_ID"
		GROUP BY TT0."TransId",TT0."TransRowId"),0)) 
		
			ELSE 
			
				(T1."Credit" - ifnull((select sum(TT0."ReconSum") from 
		"ITR1" TT0 where TT0."TransId" = T0."TransId"  AND TT0."TransRowId" = T1."Line_ID"
		GROUP BY TT0."TransId",TT0."TransRowId"),0))
				
		END)AS DECIMAL(16,2)) AS "Total",
		
		T2."WTCode" AS "CodigoRetencion" ,
		
		IFNULL(CAST( 
		(CASE 
			WHEN ifnull(T1."FCCurrency",'SOL') in ('USD','EUR') 
				THEN (T2."WTAmntFC") 
			ELSE 
				(T2."WTAmnt")
				
		END)AS DECIMAL(16,2)), 0.0) AS "Retencion",
		
		CAST( 
		(CASE 
			WHEN IFNULL(T1."FCCurrency",'SOL') in ('USD','EUR') 
				THEN 	(ifnull(T1."FCCredit",0) - ifnull((select sum(TT0."ReconSumFC") 
												 from "ITR1" TT0 where TT0."TransId" = T0."TransId"  AND TT0."TransRowId" = T1."Line_ID"
												 GROUP BY TT0."TransId",TT0."TransRowId"),0)) - 
												 
						(
							(ifnull(T1."FCCredit",0) - ifnull((select sum(TT0."ReconSumFC") 
													 from "ITR1" TT0 where TT0."TransId" = T0."TransId"  AND TT0."TransRowId" = T1."Line_ID"
												     GROUP BY TT0."TransId",TT0."TransRowId"),0)) / ((case when T1."FCCredit" = 0 then 1 else T1."FCCredit" end)) * ifnull((T2."WTAmntFC" - T2."ApplAmntFC"),0)
												 
						)						 
												 
			ELSE 
						(ifnull(T1."Credit",0) - ifnull((select sum(TT0."ReconSum") from 
						"ITR1" TT0 where TT0."TransId" = T0."TransId"  AND TT0."TransRowId" = T1."Line_ID"
						GROUP BY TT0."TransId",TT0."TransRowId"),0)) - 
						
						(
							(ifnull(T1."Credit",0) - ifnull((select sum(TT0."ReconSum") 
													 from "ITR1" TT0 where TT0."TransId" = T0."TransId"  AND TT0."TransRowId" = T1."Line_ID"
												     GROUP BY TT0."TransId",TT0."TransRowId"),0)) / ((case when T1."Credit" = 0 then 1 else T1."Credit" end)) * ifnull((T2."WTAmnt" - T2."ApplAmnt"),0)
												 
						)	
				
		END)AS DECIMAL(16,2))    AS "TotalPagar",
			
		T3."LicTradNum" AS "RUC",

		(SELECT CASE :tipoBanco WHEN '000' THEN R3."Account" ELSE CASE WHEN :tipoBanco = R3."BankCode" THEN R3."Account" ELSE R3."U_EXM_INTERBANCARIA" END END 
		FROM OCRD R0
		LEFT JOIN OCRB R3 ON R0."CardCode"=R3."CardCode" AND R3."U_EXC_ACTIVO"='Y' AND IFNULL(R3."UsrNumber1",'') = ifnull(T1."FCCurrency",'SOL') --:monedaLoc --normal
		WHERE R0."CardCode" = T3."CardCode")
		AS "Cuenta",
		
		(SELECT R3."UsrNumber1"
		FROM OCRD R0
		LEFT JOIN OCRB R3 ON R0."CardCode"=R3."CardCode" AND R3."U_EXC_ACTIVO"='Y' AND IFNULL(R3."UsrNumber1",'') = ifnull(T1."FCCurrency",'SOL') --:monedaLoc --normal
		WHERE R0."CardCode" = T3."CardCode")
		AS "CuentaMoneda",

		(SELECT R3."BankCode"
		FROM OCRD R0
		LEFT JOIN OCRB R3 ON R0."CardCode"=R3."CardCode" AND R3."U_EXC_ACTIVO"='Y' AND IFNULL(R3."UsrNumber1",'') = ifnull(T1."FCCurrency",'SOL') --:monedaLoc --normal
		WHERE R0."CardCode" = T3."CardCode")
		AS "BankCode",
		(CASE 
			WHEN DAYS_BETWEEN(T0."DueDate", :fechaVencH) < 0 THEN 0 
			ELSE DAYS_BETWEEN(T0."DueDate", :fechaVencH) 
		END) AS "Atraso",
		--0 AS "Atraso",
		'' AS "Marca",
		'' AS "Estado",
		'AS' as "Documento",
		'' AS "Comentario",
		CASE WHEN T3."QryGroup11" = 'Y' THEN 'PROV. CAJA CHICA' ELSE '' END AS "Propiedad",
		T0."DueDate"
		,'' as "Origen"
		,T1."PayBlock" AS "BloqueoPago"
		,'N' AS "DetraccionPend"
		,0 AS "NroCuota"
		,T1."Line_ID" AS "LineaAsiento"
		,T0."Memo" AS "GlosaAsiento"
		,T0."U_EXX_PROORI" as "CardCodeFactoring" 
		,T4."CardName" as "CardNameFactoring" 
		,T1."BPLId" as "CodSucursal"
		,T0.U_EXX_PRIPAG as "CodPrioridad"
	FROM 
		OJDT T0
		LEFT JOIN JDT1 T1 ON T0."TransId" = T1."TransId"
		LEFT JOIN JDT2 T2 ON T1."TransId" = T2."AbsEntry"
		--LEFT JOIN OCTG T2 ON T0."GroupNum" = T2."GroupNum"
		inner JOIN OCRD T3 ON T1."ShortName" = T3."CardCode"
		LEFT JOIN OCRD T4 ON T0."U_EXX_PROORI" = T4."CardCode"
		--LEFT JOIN OCRB T4 ON T0."CardCode" = T4."CardCode"
	WHERE 
	 	T0."TransType" = 30
	
		/*
		AND ifnull((select count(*) from "ITR1" TT0 where TT0."TransId" = T0."TransId" GROUP BY TT0."TransId"),0) = 0
		*/
		and ((T1."Credit" - 
		ifnull((select sum(TT0."ReconSum") from 
		"ITR1" TT0 where TT0."TransId" = T0."TransId" AND TT0."TransRowId" = T1."Line_ID"
		GROUP BY TT0."TransId", TT0."TransRowId"),0)) > 0 or ((T1."FCCredit" - 
		ifnull((select sum(TT0."ReconSumFC") from 
		"ITR1" TT0 where TT0."TransId" = T0."TransId" AND TT0."TransRowId" = T1."Line_ID"
		GROUP BY TT0."TransId", TT0."TransRowId"),0)) > 0))
		
		--AND T1."DueDate" <= :fechaVencH
		--AND (ifnull(T1."FCCurrency",'SOL') = UPPER(:monedaLoc) or ifnull(T1."FCCurrency",'SOL') = UPPER(:monedaExt))
		AND T3."CardCode" like '%' || :CardCode || '%'
		and ( T1."Credit" > 0 or T1."FCCredit" > 0)
		and ifnull(T1."U_EXX_DETRACCION",'') <> 'Y'
		/*AND T0."TransId" NOT IN (SELECT "U_SMC_DOCENTRY" FROM "@SMC_APM_ESCDET" 
									WHERE "U_SMC_ESCCAB" = :escenario and "U_SMC_TIPO_DOCUMENTO" = 'AS')*/
		--AND ifnull((SELECT max('Y') FROM "@SMC_APM_ESCDET" 
		--							WHERE "U_SMC_ESCCAB" = :escenario 
		--and "U_SMC_TIPO_DOCUMENTO" = 'AS' and "U_SMC_DOCENTRY" = T0."TransId" and "U_EXP_LINEAASIENTO" = T1."Line_ID"),'N') != 'Y'

		AND (ifnull((SELECT max('Y') FROM "@EXD_EPG1" TX0 inner join "@EXD_OEPG" TX1 on TX0."DocEntry" = TX1."DocEntry"
		WHERE TX0."U_TIPO_DOCUMENTO" = 'AS' and TX0."U_DOCENTRY" = T0."TransId" and TX0."U_NRO_LINEA_AS" = T1."Line_ID" 
		
		and (ifnull((select max('Y') from "@EXP_OPMP" TT0 
		inner join "@EXP_PMP1" TT1 on TT0."DocEntry" = TT1."DocEntry"
		where TT0."Status" = 'C' and TT1."U_EXP_COD_ESCENARIOPAGO" = TX1."DocEntry" and TT1."U_EXP_TIPODOC" = '30' 
		and TT1."U_EXP_DOCENTRYDOC" = TX0."U_DOCENTRY" and TT1."U_EXP_ASNROLINEA" = TX0."U_NRO_LINEA_AS" and TT1."U_EXP_ESTADO" = 'ER'),'N')) = 'N'
		
		and ifnull(TX1."Canceled",'') != 'Y'),'N') != 'Y' 
		OR T0."U_CP_VARESC"='Y')
		
		) T0
	WHERE
		T0."Total" > 0 and T0."Documento" = ifnull(nullif(:codTipoDocumento,'VR'),T0."Documento")
		and T0."FechaVencimiento" between :fechaVencD and :fechaVencH
		--and T0."BankCode" like '%'||:filtroBanco||'%' --se agrego nuevo
		and T0."CodSucursal"	= ifnull(nullif(:codSucursal,'-1'),T0."CodSucursal")
		and ifnull(T0."CodPrioridad",'') like '%'||:codPrioridad||'%'
	ORDER BY 
		T0."FechaVencimiento" desc;  
	
	select * from :DOCS TT0 inner join  
	(
		select "CodSucursal","CardCode" from :DOCS 	
		group by "CodSucursal","CardCode" 
		having sum(case when "DocCur" = 'SOL' then "TotalPagar" else "TotalPagar" * ifnull(tipoCambioP,1) end ) > ifnull(:montoMinimo,0.00)
	) AS TT1 on  TT0."CodSucursal" = TT1."CodSucursal" and TT0."CardCode" = TT1."CardCode" and TT0."TotalPagar" > 0;
END;