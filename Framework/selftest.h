/*
 * selftest.h
 *
 * Created: 2018/3/6 9:25:50
 *  Author: A41536
 */ 


#ifndef SELFTEST_H_
#define SELFTEST_H_

// *****************[ Selftest Status ]***************************

#define Selftest_OK 0x0010       // ' '
#define Selftest_Failed 0x0011   // ' '
#define FW_Version 0x0D02
#define Sleep_Message 0x0002
#define Wake_Message 0x0003
#define Enable_Message 0x0004


// *****************[ Selftest Command ]***************************

#define Selftest_Get_Status 0x30
#define Selftest_Get_Sensor_CC_Val 0x31
#define Selftest_Get_Sensor_Reference 0x32
#define Selftest_Get_Sensor_Delta 0x33
#define Selftest_Calibrate 0x34
#define Selftest_Suspend 0x35
#define Selftest_Resume 0x36
#define Selftest_Cmd_Error 0xfe
#define Selftest_Bus_Error 0xff


void self_test_process(void);
void touch_wakeup(void);

#endif /* SELFTEST_H_ */