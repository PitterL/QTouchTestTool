/*
 * selftest.c
 *
 * Created: 2018/3/6 9:34:48
 *  Author: A41536
 */ 
#include "selftest.h"
#include "usart_basic.h"
#include "touch.h"

#define FRAME_MAGICWORD 0x5a
#define LENGTH_FRAME_L 68
#define LENGTH_FRAME_S 8

bool USART_recv(uint8_t *buf, volatile int32_t timeout)
{
	while(timeout-- > 0)
	{
		if(USART_is_rx_ready()) {
			*buf = USART_read();
			return true;
		}
	}
		
	return false;
}

void uart_transmit(const uint8_t data)
{
	USART_write(data);
}

uint8_t crc8(uint8_t crc, uint8_t val)
{
	uint8_t fb;
	const uint8_t CRC_PLOY = 0x8c;
	
	for(int i = 0; i < 8; i++)
	{
		fb = (crc ^ val) & 0x01;
        val >>= 1;
        crc >>= 1;
        if (fb)
			crc ^= CRC_PLOY;
	}

    return crc;
}

uint8_t calculate_crc(const uint8_t *data, int len)
{
	uint8_t crc = 0;

    for(int i = 0; i < len; i++)
		crc = crc8(crc, data[i]);

	return crc;
}

//magw, seq, seqn, dat0, dat1, dat3, dat4, crc
enum {S_MAGW, S_SEQ, S_SEQN, S_DATA};
void send_frame8(uint8_t seq, uint8_t dat0, uint8_t dat1, bool result)
{
	uint8_t i;
	uint8_t crc;
	uint8_t buf[LENGTH_FRAME_S];
	
	buf[S_MAGW] = FRAME_MAGICWORD;  //magic word
	if (seq & 0x1)
		seq++;		//odd: long response, even: short response
	
	if (!result)
		seq += 2;	//seq not match received, that mean NAK
	
	buf[S_SEQ] = seq;
	buf[S_SEQN] = ~seq;
	buf[S_DATA] = dat0;
	buf[S_DATA + 1] = dat1;
	//buf[5:6] random data
	
	crc = calculate_crc(buf, sizeof(buf) -1);
	buf[sizeof(buf) - 1] = crc;
	
	for(i = 0; i < sizeof(buf); i++)
		uart_transmit(buf[i]);
}

uint8_t recv_frame(uint8_t * data, uint8_t len, volatile int32_t timeout)
{
	uint8_t count = 0;
	
	while(timeout-- > 0)
	{
		if(USART_is_rx_ready()) {
			data[count++] = USART_read();
			
			if (count == len)
				break;
		}
	}
	
	return count;
}

void clear_frame()
{
	while(USART_is_rx_ready())
	{
		USART_read();
	}
}

void response8(uint8_t seq, uint16_t data, bool result)
{
	send_frame8(seq, (uint8_t)data, (uint8_t)(data >> 8u), result);
}

#define OK  0
#define ETIMEOUT  1
#define EVAL 2
#define EMEM 3

enum {R_MAGW, R_SEQ, R_SEQN, R_CMD, R_ID, R_DATA};

int receive(uint8_t *data, uint8_t len)
{
	const int32_t timeout = 30000;
	uint8_t magw, seq, seqn, crc_calc;
	uint8_t count, size, size_left, off;

	if (!USART_recv(&magw, timeout))
		return -ETIMEOUT;
	
	if (magw != FRAME_MAGICWORD)
		return -EVAL;
		
	if (!USART_recv(&seq, timeout))
		return -ETIMEOUT;
	
	if (!USART_recv(&seqn, timeout))
		return -ETIMEOUT;
	
	if (seq & seqn)
		return -EVAL;
	
	if (seq & 1)
		size = LENGTH_FRAME_L;	//even short command, odd long command
	else
		size = LENGTH_FRAME_S;
	
	if (len < size)
		return -EMEM;
	
	data[R_MAGW] = magw;
	data[R_SEQ] = seq;
	data[R_SEQN] = seqn;
	off = R_SEQN + 1;
	size_left = size - off;
	
	count = recv_frame(data + off, size_left, timeout * size_left);
	if (count == size_left)
	{
		crc_calc = calculate_crc(data, size - 1);
		if (crc_calc == data[size - 1])
			return OK;
		else
			return -EVAL;
	}else
		return -ETIMEOUT;
}

void self_test_process(void)
{
	int16_t	temp_int_calc;
	uint16_t u16temp_output;
	
	uint8_t buf[LENGTH_FRAME_L];
	uint8_t seq, cmd, id;
	int res;
	
	if (USART_is_rx_ready())
	{
		do {
			res = receive(buf, sizeof(buf));
		}while(res == -EVAL);
		
		if (res == OK)
		{
			seq = buf[R_SEQ];
			cmd = buf[R_CMD];
			id = buf[R_ID];
			
			if (cmd == Selftest_Get_Sensor_CC_Val)
			{
				u16temp_output = get_sensor_cc_val(id);
			}
			else if(cmd == Selftest_Get_Sensor_Reference)
			{
				u16temp_output = get_sensor_node_reference(id);
			}
			else if(cmd == Selftest_Get_Sensor_Delta)
			{
				temp_int_calc = get_sensor_node_signal(id);
				temp_int_calc -= get_sensor_node_reference(id);
				u16temp_output = (uint16_t)(temp_int_calc);
			}
			else
			{
				res = -EVAL;
				u16temp_output = 0xffee;
			}
		}else{
			seq = 0xfe;
			u16temp_output = 0xfffe;
		}
		
		response8(seq, u16temp_output, res == OK);	
	}
}
