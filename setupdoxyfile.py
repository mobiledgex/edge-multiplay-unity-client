import yaml
import os


def main():
  f = open("config.yml", "r")
  config = yaml.load(f, Loader=yaml.Loader)
  # print( config['project-name']+
  #       config['output-directory']+
  #       config['layout-file']+
  #       config['input-directory']+
  #       config['example-path']+
  #       config['html-header']+
  #       # config['html-footer']+
  #       config['html-style-sheet']+
  #       config['html-extra-style-sheet']+
  #       config['html-extra-files'])
  os.system(
      """( cat ./config/Doxyfile ; 
    echo \"PROJECT_NAME={}\" 
    echo \"OUTPUT_DIRECTORY={}\" 
    echo \"LAYOUT_FILE={}\" 
    echo \"INPUT={}\"
    echo \"EXAMPLE_PATH={}\"
    echo \"HTML_HEADER={}\"
    echo \"HTML_STYLESHEET={}\"
    echo \"HTML_EXTRA_STYLESHEET={}\"
    echo \"HTML_EXTRA_FILES={}\"
    ) | doxygen -"""
    .format(
        config['project-name'],
        config['output-directory'],
        config['layout-file'],
        config['input-directory'],
        config['example-path'],
        config['html-header'],
        config['html-style-sheet'],
        config['html-extra-style-sheet'],
        config['html-extra-files'],
    ))


if __name__ == '__main__':
    main()
