#include "account.h"
#include <stdio.h>

int main()
{
    consoleInit(NULL);
    Result rc = accu0_Initialize();
    printf("accu0_Initialize: 0x%x\n", rc);
    if(R_SUCCEEDED(rc))
    {
        u32 count;
        rc = accu0_GetUserCount(&count);
        printf("accu0_Initialize: 0x%x\n", rc);
        if(R_SUCCEEDED(rc))
        {
            printf("User count: %d", count);
        }
    }
    while(appletMainLoop())
    {
        consoleUpdate(NULL);
    }
}