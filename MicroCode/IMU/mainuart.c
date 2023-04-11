/*
 * File:   mainuart.c
 * Author: cdoiron5
 *
 * Created on March 14, 2023, 6:36 PM
 */

// DSPIC33CK256MC506 Configuration Bit Settings

// 'C' source line config statements

// FSEC
/*
* File:   main.c
* Author: Ajee
*
*/

#pragma config ALTI2C1 = ON
// FOSCSEL
#pragma config FNOSC = FRC              // Oscillator Source Selection (Primary Oscillator (XT, HS, EC))
#pragma config IESO = ON                // Two-speed Oscillator Start-up Enable bit (Start up device with FRC, then switch to user-selected oscillator source)
// FOSC
#pragma config POSCMD = HS              // Primary Oscillator Mode Select bits (HS Crystal Oscillator Mode)
#pragma config OSCIOFNC = OFF           // OSC2 Pin Function bit (OSC2 is clock output)
#pragma config FCKSM = CSDCMD           // Clock Switching Mode bits (Both Clock switching and Fail-safe Clock Monitor are disabled)
#pragma config PLLKEN = ON              // PLL Lock Status Control (PLL lock signal will be used to disable PLL clock output if lock is lost)
#pragma config XTCFG = G3               // XT Config (24-32 MHz crystals)
#pragma config XTBST = ENABLE           // XT Boost (Boost the kick-start)

#pragma config ICS = 2

#define FCY 4000000UL
#define FP 4000000
#define BAUDRATE 9600
#define BRGVAL ((FP/BAUDRATE)/16)-1
#define DELAY_105uS asm volatile ("REPEAT, #4201"); Nop();

#include <libpic30.h>
#include "xc.h"
#include "time.h"
#include "stdio.h"

// **Functions** //
int I2C_Start();
void I2C_Stop();
void I2CRestart(void);
void I2C_Write(uint8_t I2CAdd, uint8_t RegAdd, uint8_t data);
char I2C_Read(uint8_t I2CAdd, uint8_t RegAdd);
char xlGyro;
char xhGyro;
char ylGyro;
char yhGyro;
char zlGyro;
char zhGyro;
char xlAcc;
char xhAcc;
char ylAcc;
char yhAcc;
char zlAcc;
char zhAcc;
char xhOff;
char yhOff;
char zhOff;
int xTempAcc;
int xAcc;
int yTempAcc;
int yAcc;
int zTempAcc;
int zAcc;
int xMove;
int yMove;
char xMot;
char yMot;
int freq;
char deltaX;
char deltaY;
char motreg;
int mot;


// **TX interrupt //
void __attribute__((__interrupt__)) _U1TXInterrupt(void){
    IFS0bits.U1TXIF = 0;
    //if(IFS0bits.U1TXIF == 1){
    //U1TXREGbits.TXREG = 0xF3; 
    __delay_ms(10); 
    //}
}

// **Main** //
int main(void) {
    // set up board
    TRISBbits.TRISB14 = 1;      // Set RX as input
    TRISBbits.TRISB13 = 0;      // Set TX as output
    
    // Assign RX and TX pins for UNB Dev Board
    RPCONbits.IOLOCK = 0;       // All Peripheral Remapping registers are unlocked and can be written
    RPINR18bits.U1RXR = 46;     // Assign UART1 Receive (U1RX) to RP46 pin (BM_TX)
    RPOR6bits.RP45R = 1;        // Assign RP45 (BM_RX) output pin to UART1 Transmit (U1TX)
    RPCONbits.IOLOCK = 1;       // All Peripheral Remapping registers are locked and cannot be written
    
    // UART Setup
    U1BRG = BRGVAL;             
    // UART interrupt
    INTCON2bits.GIE = 1;        // Global Interrupt Enabled
    IEC0bits.U1TXIE = 1;        // Enable UART1 Transmitter Interrupt
    // UART Transmit
    U1MODEbits.MOD = 0;         // Asynchronous 8-bit UART
    U1MODEHbits.UTXINV = 0;     // Output data are not inverted; TX output is high in Idle state
    U1MODEHbits.STSEL = 0;      // 1 Stop bit sent, 1 checked at receive
    U1MODEHbits.FLO = 0;        // Flow control off
    U1STAHbits.UTXISEL = 0;     // Triggers transmit interrupt when there are eight empty slots in the buffer; TX buffer is empty
    U1STAHbits.URXISEL = 0;     // Trigger interrupt when one word waiting in the buffer
    U1MODEbits.UARTEN = 1;      // UART is ready to transmit and receive
    U1MODEbits.UTXEN = 1;       // Transmit enabled
    U1MODEbits.URXEN = 1;       //Receive Enabled
    
    //Now, configure the I2C 1 module the way I want.
    I2C1CONLbits.I2CEN = 0;  // Disable I2C    
    I2C1BRG = 18;               // Should yield ~100 kHz clock frequency.
    I2C1CONLbits.I2CEN = 1;     // Enable the I2C module.
    __delay_ms(1);              // A 1 ms delay to allow the peripheral to start up. 
    
    DELAY_105uS     
    U1TXREG = 'a';
    __delay_ms(20); 
    
    I2C_Write(0x68, 0x1A, 0b00000000); //
    __delay_ms(10);
    I2C_Write(0x68, 0x23, 0b00000000); // FIFO enable
    __delay_ms(10);
    I2C_Write(0x68, 0x6A, 0b00000000);
    __delay_ms(10);
    I2C_Write(0x68, 0x6B, 0b00000000);
    __delay_ms(10);
    
    //I2C_Write(0x53,0x05,0b10101100);        // config byte
    //I2C_Write(0x53,0x2A,0x40);              // x resolution
    //I2C_Write(0x53,0x2B,0x40);              // y resolution
    
    xTempAcc = 0x00;
    yTempAcc = 0x00;
    zTempAcc = 0x00; 
    
    while(1){
       
        /*
        char ReceivedChar;
        
        if(U1STAbits.FERR == 1){
            continue;
        }
        if(U1STAbits.OERR == 1){
            U1STAbits.OERR = 0;
            continue;
        }
        if(U1STAHbits.URXBE == 0){
            ReceivedChar = U1RXREG;
        }
        */
        //I2C_Write(0x53,0x2A,0x40);              // x resolution
        //I2C_Read(0x53,0x17);
        
        // gyroscope
        I2C_Write(0x68, 0x6B, 0b00000000);
        xhGyro = I2C_Read(0x68, 0x43);
        xlGyro = I2C_Read(0x68, 0x44);
        yhGyro = I2C_Read(0x68, 0x45);
        ylGyro = I2C_Read(0x68, 0x46);
        zhGyro = I2C_Read(0x68, 0x47);
        zlGyro = I2C_Read(0x68, 0x48);
        
        
        /*
        xhAcc = I2C_Read(0x68, 0x3B);
        xlAcc = I2C_Read(0x68, 0x3C);
        yhAcc = I2C_Read(0x68, 0x3D);
        ylAcc = I2C_Read(0x68, 0x3E);
        zhAcc = I2C_Read(0x68, 0x3F);
        zlAcc = I2C_Read(0x68, 0x40);
        */
       // xhOff = xhAcc - 0x02;
       // yhOff = yhAcc - 0xFE;
       // zhOff = zhAcc - 0x3F;
        
        //xAcc = ((xhAcc << 8) & 0xFF00) + xlAcc;
        //yAcc = ((yhAcc << 8) & 0xFF00) + ylAcc;
        //zAcc = ((zhAcc << 8) & 0xFF00) + zlAcc;
       /*
        TXDelta(ylAcc, yhOff);
        TXDelta(zlAcc, zhOff);*/
        //TXDelta(xhAcc, yhAcc);
        
        
        TXDelta(0x00, xhGyro);
        TXDelta(0x0l, xlGyro);
        TXDelta(0x02, yhGyro);
        TXDelta(0x03, ylGyro);
        TXDelta(0x04, zhGyro);
        TXDelta(0x05, zlGyro);
        //TXDelta(xlAcc, xhAcc);
        
   }
//    }
    return 0;
}


// I2C Functions //
int I2C_Start() {
    I2C1STATbits.ACKSTAT = 1; // reset ack
    I2C1CONLbits.SEN = 1;   // I2C start sequence
    while(I2C1CONLbits.SEN);
    return 1;
}

void I2C_Stop() {
    I2C1CONLbits.RCEN = 0;
    I2C1CONLbits.PEN = 1; 
    while(I2C1CONLbits.PEN);
    __delay_ms(1);
}

void I2C_Restart(void)
{
    I2C1CONLbits.RSEN = 1;       // Repeated Start Condition
    while(I2C1CONLbits.RSEN);
    __delay_ms(1);
    //I2C1CONLbits.ACKDT = 0;      // Send ACK
    //I2C1STATbits.TBF = 0;       // I2C1TRN is empty
}

void I2C_Write(uint8_t I2CAdd, uint8_t RegAdd, uint8_t data) {
    I2C_Start();
    I2C1TRNbits.I2CTXDATA = (I2CAdd << 1) & 0xFE;
    while(I2C1STATbits.ACKSTAT); //Wait for ack
    while(I2C1STATbits.TRSTAT); //wait until transaction complete
  
    I2C1TRNbits.I2CTXDATA = (RegAdd);
    while(I2C1STATbits.ACKSTAT); //Wait for ack
    while(I2C1STATbits.TRSTAT); //wait until transaction complete
    
    I2C1TRNbits.I2CTXDATA = (data);
    while(I2C1STATbits.ACKSTAT); //Wait for ack
    while(I2C1STATbits.TRSTAT); //wait until transaction complete
    I2C_Stop();
}

char I2C_Read(uint8_t I2CAdd, uint8_t RegAdd) {
    char value;
    I2C_Start();
    I2C1TRNbits.I2CTXDATA = (I2CAdd << 1) & 0xFE;
    while(I2C1STATbits.ACKSTAT); //Wait for ack
    while(I2C1STATbits.TRSTAT); //wait until transaction complete
    
    I2C1TRNbits.I2CTXDATA = (RegAdd);
    while(I2C1STATbits.ACKSTAT); //Wait for ack
    while(I2C1STATbits.TRSTAT); //wait until transaction complete
    //I2C_Stop();
    I2C_Restart();
    I2C1TRNbits.I2CTXDATA = ((I2CAdd << 1) | 0x01);
    while(I2C1STATbits.ACKSTAT); //Wait for ack
    while(I2C1STATbits.TRSTAT); //wait until transaction complete
    
    I2C1CONLbits.RCEN = 1;
    while(!I2C1STATbits.RBF);
    while(I2C1CONLbits.RCEN); //the RCEN reg is cleared automatically
    //while(!(I2C1STATbits.ACKSTAT)); // Wait for nack
    I2C1CONLbits.ACKDT = 0x1;  //set the nack
    I2C1CONLbits.ACKEN = 0x1;  //set that you send an nack
    while(I2C1CONLbits.ACKEN); //waiting for it to clear
    I2C_Stop();
    value = I2C1RCV;
    return value;
}

void TXDelta(uint8_t x, uint8_t y){
    __delay_ms(20);
    U1TXREG = x;
    __delay_ms(20); 
    U1TXREG = y;
    __delay_ms(20); 
}