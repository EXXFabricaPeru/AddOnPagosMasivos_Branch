CREATE PROCEDURE EXD_SP_PMP_CALCULAR_APLICA_RETENCION
(
	docEntry int
)
AS
BEGIN
		declare tcPM decimal(19,6);
		select ifnull(max(U_EXP_TIPODECAMBIO),0.00) into tcPM from "@EXP_OPMP" where "DocEntry" = :docEntry;
		
		update "@EXP_PMP1" set U_EXP_ESTADO = ''			where "DocEntry" = :docEntry and ifnull(U_EXP_ESTADO,'') = '';
		update "@EXP_PMP1" set U_EXP_ESTADO2 = ''			where "DocEntry" = :docEntry and ifnull(U_EXP_ESTADO2,'') = '';
		update "@EXP_PMP1" set U_EXP_CARDCODE_FACTO = ''	where "DocEntry" = :docEntry and ifnull(U_EXP_CARDCODE_FACTO,'') = '';
		update "@EXP_PMP1" set U_EXP_CARDNAME_FACTO = ''	where "DocEntry" = :docEntry and ifnull(U_EXP_CARDNAME_FACTO,'') = '';
		
		update "@EXP_PMP1" set 
			U_EXP_APLICA_RETENCION	= U_EXP_APL_RETENCION_AUX,
			U_EXP_IMPORTE			= U_EXP_IMPORTE_AUX,
			U_EXP_IMPRETENCION		= 0.00,
			U_EXP_CODRETENCION		= ''
		where "DocEntry" = :docEntry and U_EXP_APL_RETENCION_AUX <> 'Y';
		
		update "@EXP_PMP1" T0 
			set T0.U_EXP_APLICA_RETENCION = 'Y'
			,T0.U_EXP_CODRETENCION = 'RIGV'
			,T0.U_EXP_IMPORTE = T0.U_EXP_IMPORTE_AUX - (T0.U_EXP_IMPORTE_AUX * 0.03)
			,T0.U_EXP_IMPRETENCION = T0.U_EXP_IMPORTE_AUX * 0.03
		from 
		 "@EXP_PMP1" T0
		 inner join
		 (
			select 
				TX1.U_EXP_COD_SUCURSAL,TX1.U_EXP_CARDCODE,TX1.U_EXP_MONEDA
			from "@EXP_OPMP" TX0 inner join "@EXP_PMP1" TX1
			on TX0."DocEntry" = TX1."DocEntry"
			where TX0."DocEntry" = :docEntry
			--and coalesce(TX1.U_EXP_APL_RETENCION_AUX,'') <> 'Y' 
			and TX1.U_EXP_AFECTO_RETENCION = 'Y'
			and TX1.U_EXP_SLC_PAGO = 'Y'
			group by TX1.U_EXP_COD_SUCURSAL,TX1.U_EXP_CARDCODE,TX1.U_EXP_MONEDA
			having case when TX1.U_EXP_MONEDA = 'SOL' then sum(TX1.U_EXP_IMPORTE + TX1.U_EXP_IMPRETENCION) 
			else sum(TX1.U_EXP_IMPORTE + TX1.U_EXP_IMPRETENCION) * :tcPM end > 700
		) T1 on T0.U_EXP_COD_SUCURSAL = T1.U_EXP_COD_SUCURSAL 
		and T1.U_EXP_CARDCODE = T0.U_EXP_CARDCODE
		and T1.U_EXP_MONEDA = T0.U_EXP_MONEDA
		where T0."DocEntry" = :docEntry and T0.U_EXP_SLC_PAGO = 'Y' 
		and T0.U_EXP_AFECTO_RETENCION = 'Y' and T0.U_EXP_APL_RETENCION_AUX <> 'Y';
END;