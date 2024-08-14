DROP FUNCTION if exists public.rsr_f_add_station_to_settlement(int8, int8);

CREATE OR REPLACE FUNCTION public.rsr_f_add_station_to_settlement(p_settlement_id bigint, p_station_id bigint)
    RETURNS void
    LANGUAGE plpgsql
AS
$function$
begin
    delete from public.settlement_stations ss where ss.station_id = p_station_id;
    insert into public.settlement_stations (settlement_id, station_id)
    values (p_settlement_id, p_station_id);
end;
$function$
;

-- Permissions

ALTER FUNCTION public.rsr_f_add_station_to_settlement(int8, int8) OWNER TO dbo;
GRANT ALL ON FUNCTION public.rsr_f_add_station_to_settlement(int8, int8) TO dbo;