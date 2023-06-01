-- FUNCTION: public."usp_GetMessages"(text, text, text, text)

-- DROP FUNCTION public."usp_GetMessages"(text, text);

CREATE OR REPLACE FUNCTION public."usp_GetMessages"(
	fromuserid text,
	touserid text,
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
 where "IsDraft" = false  AND 
 (fromUserid = ANY (string_to_array(u."ReceiverUserIds",',')) AND  touserid = u."SenderUserIds")
 OR (touserid = ANY (string_to_array(u."ReceiverUserIds",',')) AND  fromUserid = u."SenderUserIds")
 And (u."DeletedbyReceiver" is null or NOT (fromUserid = ANY (string_to_array(u."DeletedbyReceiver",',')))) 
 AND (u."DeletedFromTrash" IS NULL OR NOT (fromUserid = ANY (string_to_array(u."DeletedFromTrash",','))))
 AND (u."DeletedbySender" IS NULL OR NOT (fromUserid = ANY (string_to_array(u."DeletedbySender",','))))
 And (u."DeletedbyReceiver" is null or NOT (touserid = ANY (string_to_array(u."DeletedbyReceiver",',')))) 
 AND (u."DeletedFromTrash" IS NULL OR NOT (touserid = ANY (string_to_array(u."DeletedFromTrash",','))))
 AND (u."DeletedbySender" IS NULL OR NOT (touserid = ANY (string_to_array(u."DeletedbySender",','))))
 ORDER BY u."CreatedOn" desc;

END;
$BODY$;
--ALTER FUNCTION public."usp_GetMessages"(text, text, text, text)
  --  OWNER TO postgres;