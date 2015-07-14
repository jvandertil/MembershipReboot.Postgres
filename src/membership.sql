-- Table: groups

-- DROP TABLE groups;

CREATE TABLE groups
(
  id uuid NOT NULL,
  tenant character varying(50) NOT NULL,
  name character varying(100) NOT NULL,
  created timestamp with time zone,
  last_updated timestamp with time zone NOT NULL,
  children uuid[] NOT NULL,
  CONSTRAINT pk_groups_id PRIMARY KEY (id),
  CONSTRAINT uq_groups_tenant_name UNIQUE (tenant, name)
)
WITH (
  OIDS=FALSE
);
ALTER TABLE groups
  OWNER TO postgres;

-- Table: useraccounts

-- DROP TABLE useraccounts;

CREATE TABLE useraccounts
(
  id uuid NOT NULL,
  tenant character varying(50) NOT NULL,
  username character varying(255) NOT NULL,
  email character varying(255) NOT NULL,
  hashed_password character varying(200) NOT NULL,
  account jsonb NOT NULL,
  CONSTRAINT pk_useraccounts_id PRIMARY KEY (id)
)
WITH (
  OIDS=FALSE
);
ALTER TABLE useraccounts
  OWNER TO postgres;
