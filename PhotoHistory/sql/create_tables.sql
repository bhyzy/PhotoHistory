DROP TABLE IF EXISTS Users CASCADE;
DROP TABLE IF EXISTS Categories CASCADE;
DROP TABLE IF EXISTS Albums CASCADE;
DROP TABLE IF EXISTS Photos CASCADE;
DROP TABLE IF EXISTS TrustedUsers CASCADE;
DROP TABLE IF EXISTS Comments CASCADE;
DROP TABLE IF EXISTS Subscriptions CASCADE;
DROP TABLE IF EXISTS Activations CASCADE;
DROP TABLE IF EXISTS Votes CASCADE;

CREATE TABLE Users
(
  user_id serial primary key,
  login character varying(255) NOT NULL,
  password character varying(255) NOT NULL,
  email character varying(255) NOT NULL,
  activation_code char(32) DEFAULT NULL,
  date_of_birth date DEFAULT NULL,
  about text DEFAULT NULL,
  notify_comment boolean NOT NULL,
  notify_photo boolean NOT NULL,
  notify_subscr boolean NOT NULL
);


CREATE TABLE Categories
(
	category_id serial primary key,
	name varchar(255) NOT NULL
);


CREATE TABLE Albums
(
	album_id serial primary key,
	user_id integer NOT NULL REFERENCES Users, 
	category_id integer NOT NULL REFERENCES Categories,
	name varchar(255) NOT NULL,
	description text,
	rating smallint,
	views integer default 0,
	next_notification timestamp,
	public boolean NOT NULL,
	password varchar(255),
	comments_allow boolean NOT NULL,
	comments_auth boolean NOT NULL,
	notification_period integer
);

CREATE INDEX album_user_idx on Albums (user_id);
CREATE INDEX album_cat_idx on Albums (category_id);
CREATE INDEX album_rat_idx on Albums (rating);


CREATE TABLE Photos
(
	photo_id serial primary key,
	album_id integer NOT NULL REFERENCES Albums,
	date_taken timestamp NOT NULL,
	description text,
	file_path text NOT NULL,
	--location geography(point,4326)
	loc_latitude numeric(10,7) CHECK (loc_latitude >= -90 AND loc_latitude <= 90),
	loc_longitude numeric(10,7) CHECK (loc_longitude >= -180 AND loc_longitude <= 180)
);

CREATE INDEX photo_album_idx on Photos (album_id);
CREATE INDEX photo_date_idx on Photos(date_taken);


CREATE TABLE TrustedUsers
(
	album_id integer REFERENCES Albums,
	user_id integer REFERENCES Users,
	primary key(user_id, album_id)
);


CREATE TABLE Comments
(
	comment_id serial primary key,
	album_id integer NOT NULL REFERENCES Albums,
	user_id integer NOT NULL REFERENCES Users,
	date_posted timestamp NOT NULL,
	"body" varchar(1000) NOT NULL,
	accepted boolean NOT NULL 	
);

CREATE INDEX comment_user_idx on Comments (user_id);
CREATE INDEX comment_album_idx on Comments (album_id);



CREATE TABLE Subscriptions
(
	user_id integer REFERENCES Users,
	album_id integer REFERENCES Albums,
	primary key(user_id, album_id)
);	



CREATE TABLE Votes
(
	user_id integer REFERENCES Users,
	album_id integer REFERENCES Albums,
	primary key(user_id, album_id),
	up boolean NOT NULL
);	
CREATE INDEX vote_album_idx on Votes (album_id);