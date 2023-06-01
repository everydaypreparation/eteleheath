-- FUNCTION: public."usp_GetInbox"(text, text, text)

-- DROP FUNCTION public."usp_GetInbox"(bigint);

CREATE OR REPLACE FUNCTION public."usp_GetInbox"(
	userid text,
	secretkey text,
	algotype text)
    RETURNS TABLE(firstname text, lastname text, messageid uuid, subject text, messagestext text, senderuserids text, receiveruserids text, createdon timestamp without time zone, emailaddress character varying, readby text) 
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    ROWS 1000
    
AS $BODY$
BEGIN
RETURN QUERY
 select emro_sym_decrypt(us."Name",secretKey,algotype) as "FirstName",emro_sym_decrypt(us."Surname",secretKey,algotype) as "LastName",u."UserMessagesId" as "MessageId",
 u."Subject",u."MessagesText",u."SenderUserIds",u."ReceiverUserIds",u."CreatedOn",us."EmailAddress",u."ReadBy"
 from public."UserMessages" u JOIN public."Users" us 
 ON u."SenderUserIds" = us."UniqueUserId"::text
 where "IsDraft" = false  AND  userId::text = ANY (string_to_array(u."ReceiverUserIds",','))
 and (u."DeletedbyReceiver" is null or NOT (userId::text = ANY (string_to_array(u."DeletedbyReceiver",','))))
 AND (u."DeletedFromTrash" IS NULL OR NOT (userId::text = ANY (string_to_array(u."DeletedFromTrash",','))))
 ORDER BY u."CreatedOn" desc;
 
 --where u."ReceiverUserIds" like '%'||userId::text||'%' and "IsDraft" = false 
-- and (u."DeletedbyReceiver" is null or u."DeletedbyReceiver" not like  '%'||userId::text||'%')
 --AND (u."DeletedFromTrash" IS NULL OR u."DeletedbyReceiver" NOT LIKE  '%'||userId::text||'%');

END;
$BODY$;

--ALTER FUNCTION public."usp_GetInbox"(text, text, text)
  --  OWNER TO postgres;
