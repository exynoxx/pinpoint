#ifndef CSHARPCOMMUNICATOR_H
#define CSHARPCOMMUNICATOR_H

#include <list>
#include <string>
#include <iostream>
#include <fstream>

using namespace std;

class CSharpCommunicator
{
public:
    CSharpCommunicator(){}

    list<string> SendText(const string& str){

        static string pipeName = "mypipe";  // Make pipeName static for persistent connection
        static ifstream pipeReader;  // Make pipeReader static to avoid opening/closing on each call

        // Open the named pipe for reading if it's not already open
        if (!pipeReader.is_open()) {
            pipeReader.open(pipeName);
            if (!pipeReader.is_open()) {
                cerr << "Failed to open the named pipe for reading." << endl;
                return {};  // Return an empty list to indicate an error
            }
        }

        // Open the named pipe for writing
        ofstream pipeWriter(pipeName);

        if (!pipeWriter.is_open()) {
            cerr << "Failed to open the named pipe for writing." << endl;
            return {};  // Return an empty list to indicate an error
        }

        // Write the data to the named pipe
        pipeWriter << str << endl;
        pipeWriter.close();
        // Read the response
        list<string> responseList;
        string responseData;

        while (getline(pipeReader, responseData)) {
            responseList.push_back(responseData);
        }

        return responseList;

        list<string> emptyList;
        return emptyList;
    }
};

#endif // CSHARPCOMMUNICATOR_H
