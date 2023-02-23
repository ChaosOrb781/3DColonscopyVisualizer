import os, sys, getopt, cv2

def Video2Frames(inputfile, outputfolder):
    # Remove older version of the processed video if it exists
    if (os.path.exists(outputfolder)):
        os.rmtree(outputfolder)
    os.makedirs(outputfolder)
    
    # Get the number of frames in the video
    video = cv2.VideoCapture(inputfile)
    num_frames = int(video.get(cv2.CAP_PROP_FRAME_COUNT))
    
    for i in range(num_frames):
        # Iterate through each frame and write it back to the output dir
        ret, frame = video.read()
    
        if ret:
            # Save the frame as a JPEG image file
            filename = os.path.join(outputfolder, f"frame{i}.jpg")
            cv2.imwrite(filename, frame)
        else:
            print("Could not read the image frame")
            sys.exit()
        print(f"Processing {i+1}/{num_frames} frames")
    
    # Release the video file handle
    video.release()


if __name__ == "__main__":
    inputfile = ''
    outputfolder = ''
    opts, args = getopt.getopt(sys.argv[1:], "hi:o:", ["input=", "output="])
    for opt, arg in opts:
        if opt == '-h':
            print ('python ProcessVideo.py -i <inputfile> -o <outputfolder>')
            sys.exit()
        elif opt in ("-i"):
            inputfile = arg
        elif opt in ("-o"):
            outputfolder = arg
    print('Input file is', inputfile)
    print('Output folder is', outputfolder)
    Video2Frames(inputfile, outputfolder)