#pragma once

#define PREFIX_REQUEST				0x01
#define PREFIX_RESPONSE				0x02
#define PREFIX_CHALLENGE			0x03
#define PREFIX_CHALLENGE_RESPONSE	0x04
#define PREFIX_PAYLOAD				0x05
#define PREFIX_KEEP_ALIVE			0x06
#define PREFIX_DISCONNECT			0x07

#define PAYLOAD_MAX_SIZE			1200

#define CONNECT_TOKEN_SIZE			8
#define RESPONSE_DATA_SIZE			16
#define CHALLENGE_DATA_SIZE			512

typedef struct REQUEST_P {
	char prefix;
	char protocol[4];
	char connect_token[CONNECT_TOKEN_SIZE];
};

typedef struct RESPONSE_P {
	char prefix;
	uint64_t seq;
	uint64_t ack;
	char response[RESPONSE_DATA_SIZE];
};


typedef struct CHALLENGE_P {
	char prefix = PREFIX_CHALLENGE;
	uint64_t seq;
	char challenge_data[CHALLENGE_DATA_SIZE];
};

typedef struct CHALLENGE_RESPONSE_P {
	char prefix = PREFIX_CHALLENGE_RESPONSE;
	uint64_t seq;
	uint64_t ack;
	char challenge_data[CHALLENGE_DATA_SIZE];
};


typedef struct KEEP_ALIVE_P {
	char prefix = PREFIX_KEEP_ALIVE;
	uint64_t seq;
	uint64_t ack;
};

typedef struct PAYLOAD {
	char prefix = PREFIX_PAYLOAD;
	uint64_t seq;
	uint64_t ack;
	char data[];//maxsize of 1200
};