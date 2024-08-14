DROP FUNCTION if exists public.rsr_f_add_region_to_country(int8, int8);

CREATE OR REPLACE FUNCTION public.rsr_f_add_region_to_country(p_country_id bigint, p_region_id bigint)
    RETURNS void
    LANGUAGE plpgsql
AS
$function$

begin
    delete from public.country_regions cr where cr.regionid = p_region_id;
    insert into public.country_regions (countryid, regionid)
    values (p_country_id, p_region_id);
end;
$function$
;

-- Permissions

ALTER FUNCTION public.rsr_f_add_region_to_country(int8, int8) OWNER TO dbo;
GRANT ALL ON FUNCTION public.rsr_f_add_region_to_country(int8, int8) TO dbo;