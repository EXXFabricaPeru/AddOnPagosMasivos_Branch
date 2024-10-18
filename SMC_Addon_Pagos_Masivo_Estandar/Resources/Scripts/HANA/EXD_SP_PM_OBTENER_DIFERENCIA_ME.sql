CREATE PROCEDURE EXD_SP_PM_OBTENER_DIFERENCIA_ME
(
	docEntryPM int,
	codSucursal int,
	codCtaBanco varchar(20)
)
AS
BEGIN
	declare cntAnulados1 int;
	declare cntAnulados2 int;
	
	select count('A') into cntAnulados1 from OVPM T0 
	inner join(
	select distinct 
		U_EXP_NROPGOEFEC2
	from "@EXP_PMP1" TX0 where TX0."DocEntry" = :docEntryPM 
	and TX0."U_EXP_COD_SUCURSAL" = :codSucursal
	and TX0."U_EXP_CODCTABANCO" = :codCtaBanco
	and TX0."U_EXP_MONEDA_PAGO" = 'USD'
	and TX0."U_EXP_SLC_PAGO" = 'Y') TT
	on T0."DocEntry" = TT.U_EXP_NROPGOEFEC2
	where T0."Canceled" = 'N';
	
	select count('A') into cntAnulados2 from OVPM T0 
	inner join(
	select distinct 
		U_EXP_NROPGOEFEC 
	from "@EXP_PMP1" TX0 where TX0."DocEntry" = :docEntryPM
	and TX0."U_EXP_COD_SUCURSAL" = :codSucursal
	and TX0."U_EXP_CODCTABANCO" = :codCtaBanco
	and TX0."U_EXP_MONEDA_PAGO" = 'USD'
	and TX0."U_EXP_SLC_PAGO" = 'Y') TT
	on T0."DocEntry" = TT.U_EXP_NROPGOEFEC
	where T0."Canceled" = 'N';
	
	IF cntAnulados1 = 0 AND cntAnulados2 = 0
	THEN
		select SUM("Total") from
		(
			select sum(T1."Debit") as "Total" from OVPM T0 
			inner join JDT1 T1 on T0."TransId" 	= T1."TransId"
			inner join OACT T2 on T2."AcctCode"	= T1."Account"
			inner join OBPL T3 on T0."BPLId"	= T3."BPLId" and T2."FormatCode" = T3."U_EXD_CTAPTEPGOMSV"
			inner join(
			select distinct 
				U_EXP_NROPGOEFEC2
			from "@EXP_PMP1" TX0 where TX0."DocEntry" = :docEntryPM 
			and TX0."U_EXP_COD_SUCURSAL" = :codSucursal
			and TX0."U_EXP_CODCTABANCO" = :codCtaBanco
			and TX0."U_EXP_MONEDA_PAGO" = 'USD'
			and TX0."U_EXP_SLC_PAGO" = 'Y') TT
			on T0."DocEntry" = TT.U_EXP_NROPGOEFEC2
			where T0."Canceled" = 'N'
			
			union all 
			
			select sum(T1."Credit") * -1 from OVPM T0 
			inner join JDT1 T1 on T0."TransId" 	= T1."TransId"
			inner join OACT T2 on T2."AcctCode"	= T1."Account"
			inner join OBPL T3 on T0."BPLId"	= T3."BPLId" and T2."FormatCode" = T3."U_EXD_CTAPTEPGOMSV"
			inner join(
			select distinct 
				U_EXP_NROPGOEFEC 
			from "@EXP_PMP1" TX0 where TX0."DocEntry" = :docEntryPM
			and TX0."U_EXP_COD_SUCURSAL" = :codSucursal
			and TX0."U_EXP_CODCTABANCO" = :codCtaBanco
			and TX0."U_EXP_MONEDA_PAGO" = 'USD'
			and TX0."U_EXP_SLC_PAGO" = 'Y') TT
			on T0."DocEntry" = TT.U_EXP_NROPGOEFEC
			where T0."Canceled" = 'N'
		);
	ELSE
		select 0.00 as  "Total" from DUMMY;
	END IF;
END;