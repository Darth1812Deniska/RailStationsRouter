DROP FUNCTION if exists public.rsr_f_add_settlement_to_region(int8, int8);

CREATE OR REPLACE FUNCTION public.rsr_f_add_settlement_to_region(p_region_id bigint, p_settlement_id bigint)
    RETURNS void
    LANGUAGE plpgsql
AS
$function$
begin
    delete from public.region_settlements rs where rs.settlementid = p_settlement_id;
    insert into public.region_settlements (regionid, settlementid)
    values (p_region_id, p_settlement_id);
end;
$function$
;

-- Permissions

ALTER FUNCTION public.rsr_f_add_settlement_to_region(int8, int8) OWNER TO dbo;
GRANT ALL ON FUNCTION public.rsr_f_add_settlement_to_region(int8, int8) TO dbo;